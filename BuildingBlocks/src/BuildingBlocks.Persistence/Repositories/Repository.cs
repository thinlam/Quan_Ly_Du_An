using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using BuildingBlocks.CrossCutting.Exceptions;
using BuildingBlocks.Domain.Constants;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Persistence.Repositories {
    public class Repository<TEntity, TKey>(DbContext dbContext) : IRepository<TEntity, TKey>
    where TEntity : class, IHasKey<TKey>, IAggregateRoot, new()
    where TKey : notnull {
        private readonly DbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private DbSet<TEntity> DbSet => _dbContext.Set<TEntity>();

        // ✅ Static cache cho reflection operations để tối ưu performance
        private static readonly ConcurrentDictionary<Type, PropertyInfo?> _isDeletedPropertyCache = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo?> _usedPropertyCache = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo?> _pathPropertyCache = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo?> _levelPropertyCache = new();
        private static readonly ConcurrentDictionary<string, MethodInfo> _queryableMethodCache = new();
        private static readonly ConcurrentDictionary<string, bool> _implementsInterfaceCache = new();

        #region Properties
        public IUnitOfWork UnitOfWork {
            get => (IUnitOfWork)_dbContext;
        }
        #endregion

        #region CRUD Operations
        public async Task AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
            if (entity.Id!.Equals(default(TKey))) {
                await AddAsync(entity, cancellationToken);
            } else {
                await UpdateAsync(entity, cancellationToken);
            }
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) {
            await DbSet.AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
            await DbSet.AddRangeAsync(entities, cancellationToken);
        }

        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
            DbSet.Update(entity);
            return Task.CompletedTask;
        }

        public void Delete(TEntity entity) {
            DbSet.Remove(entity);
        }
        #endregion

        #region Querying
        public IQueryable<TEntity> GetQueryableSet(
        bool OnlyUsed = true,
        bool OnlyNotDeleted = true,
        bool OrderByIndex = true
        ) => HandleFilter(OnlyUsed: OnlyUsed, OnlyNotDeleted: OnlyNotDeleted, OrderByIndex: OrderByIndex);
        public IQueryable<TEntity> GetQueryableSet() => HandleFilter();
        private IQueryable<TEntity> HandleFilter(
            //Còn sử dụng
            bool OnlyUsed = true,
            //Chưa xoá
            bool OnlyNotDeleted = true,
            //Sắp xếp ưu tiên tạo mới nhất
            bool OrderByIndex = true
        ) {
            IQueryable<TEntity> query = DbSet;
            if (OnlyNotDeleted) {
                // ✅ Cached soft delete filter - tối ưu performance
                var isDeletedProperty = Repository<TEntity, TKey>.GetCachedIsDeletedProperty();
                if (isDeletedProperty != null) {
                    var parameter = Expression.Parameter(typeof(TEntity), "e");
                    var property = Expression.Property(parameter, EntityPropertyNames.IsDeleted);
                    var notExpression = Expression.Not(property);
                    var whereLambda = Expression.Lambda<Func<TEntity, bool>>(notExpression, parameter);

                    var whereMethod = Repository<TEntity, TKey>.GetCachedWhereMethod();
                    query = (IQueryable<TEntity>)whereMethod.Invoke(null, [query, whereLambda])!;
                }
            }
            if (OnlyUsed) {
                // ✅ Cached Used filter - tối ưu performance
                var usedProperty = Repository<TEntity, TKey>.GetCacheUsedProperty();
                if (usedProperty != null) {
                    var parameter = Expression.Parameter(typeof(TEntity), "e");
                    var property = Expression.Property(parameter, EntityPropertyNames.Used);
                    var trueConstant = Expression.Constant(true);
                    var equalExpression = Expression.Equal(property, trueConstant);
                    var whereLambda = Expression.Lambda<Func<TEntity, bool>>(equalExpression, parameter);
                    var whereMethod = Repository<TEntity, TKey>.GetCachedWhereMethod();
                    query = (IQueryable<TEntity>)whereMethod.Invoke(null, [query, whereLambda])!;
                }
            }
            if (OrderByIndex) {
                // ✅ Cached interface check - tối ưu performance
                if (Repository<TEntity, TKey>.IsImplementsInterface(typeof(TEntity), typeof(IUnixTimeIndex))) {
                    var parameter = Expression.Parameter(typeof(TEntity), "e");
                    var property = Expression.Property(parameter, nameof(IUnixTimeIndex.Index));
                    var lambda = Expression.Lambda(property, parameter);
                    var orderByDescendingMethod = Repository<TEntity, TKey>.GetCachedOrderByDescendingMethod();
                    query = (IQueryable<TEntity>)orderByDescendingMethod.Invoke(null, [query, lambda])!;
                }
            }
            return query;
        }
        public IQueryable<TEntity> GetOriginalSet() => HandleFilter(OnlyUsed: false, OnlyNotDeleted: false);
        public IQueryable<TEntity> GetOrderedSet() => HandleFilter(OnlyUsed: false, OrderByIndex: true);
        #endregion

        #region Bulk Operations
        public void BulkInsert(IEnumerable<TEntity> entities, bool keepIdentity = false) {
            _dbContext.BulkInsert(entities, (configureOptions) => { configureOptions.KeepIdentity = keepIdentity; });
        }

        public void BulkInsert(IEnumerable<TEntity> entities, Expression<Func<TEntity, object>> columnNamesSelector) {
            _dbContext.BulkInsert(entities, columnNamesSelector);
        }

        public void BulkUpdate(IEnumerable<TEntity> entities, Expression<Func<TEntity, object>> columnNamesSelector) {
            _dbContext.BulkUpdate(entities, columnNamesSelector);
        }

        public void BulkDelete(IEnumerable<TEntity> entities) {
            _dbContext.BulkDelete(entities);
        }

        public void BulkMerge(IEnumerable<TEntity> entities, Expression<Func<TEntity, object>> idSelector,
            Expression<Func<TEntity, object>> updateColumnNamesSelector, Expression<Func<TEntity, object>> insertColumnNamesSelector) {
            _dbContext.BulkMerge(entities, idSelector, updateColumnNamesSelector, insertColumnNamesSelector);
        }
        #endregion

        #region MaterializedPath Operations
        /// <summary>
        /// Tổ tiên
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<IMaterializedPathEntity<TKey>>> GetAncestorsAsync(TKey id, CancellationToken cancellationToken = default) {
            var entity = await DbSet.AsNoTracking().FirstOrDefaultAsync(e => Equals(id, e.Id), cancellationToken);
            if (entity == null) return [];
            if (IsMaterializedPath()) {
                IMaterializedPathEntity<TKey> node = (IMaterializedPathEntity<TKey>)entity;
                var ids = node!.Path?
                    .Trim('/')
                    .Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => (TKey)Convert.ChangeType(x, typeof(TKey)))
                    .ToList();

                // Exclude self from ancestors
                if (ids != null && ids.Count > 0 && Equals(ids.Last(), id)) {
                    ids.RemoveAt(ids.Count - 1);
                }

                if (ids == null || ids.Count == 0)
                    return [];

                var entities = await DbSet
                    .Where(e => ids.Contains(e.Id))
                    .Cast<IMaterializedPathEntity<TKey>>()
                    .OrderBy(e => e.Level)
                    .ToListAsync(cancellationToken);

                return [.. entities];
            }
            return [];
        }

        /// <summary>
        /// Con cháu / hậu duệ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<IMaterializedPathEntity<TKey>>> GetDescendantsAsync(TKey id, CancellationToken cancellationToken = default) {
            if (IsMaterializedPath()) {
                var prefix = $"/{id}/";
                var entities = await DbSet
                    .Cast<IMaterializedPathEntity<TKey>>()
                    .Where(e => EF.Functions.Like(e.Path, prefix + "%"))
                    .OrderBy(e => e.Level)
                    .ToListAsync(cancellationToken);

                return [.. entities];
            }

            return [];

        }

        public void ResetMaterializedPathsInMemory(
            List<TEntity> allNodes,
            Func<TEntity, TKey> getId,
            Func<TEntity, TKey?> getParentId,
            Action<TEntity, string> setPath,
            Action<TEntity, int> setLevel,
            Action<TEntity, TKey?> setParentId) {

            var nodeMap = allNodes.ToDictionary(getId);

            // BƯỚC 1: Xử lý Self-Reference (Node tự làm cha của chính nó)
            // Nếu phát hiện, buộc node đó trở thành node gốc.
            foreach (var node in allNodes) {
                var parentId = getParentId(node);
                if (parentId != null && getId(node).Equals(parentId)) {
                    // Gán lại ParentId thành null/default để biến nó thành node gốc
                    setParentId(node, default);
                    setPath(node, $"/{getId(node)}/");
                    setLevel(node, 0);
                }
            }

            // BƯỚC 2: Phát hiện và phá vỡ Circular-Reference (Tham chiếu vòng)
            // Ví dụ: A -> B -> A. Chúng ta sẽ phá vỡ bằng cách biến A thành node gốc.
            foreach (var startNode in allNodes) {
                var path = new HashSet<TKey>();
                var currentNode = startNode;

                // Bắt đầu đi ngược từ node hiện tại lên các node cha
                while (currentNode != null) {
                    var currentId = getId(currentNode);

                    // Nếu ID hiện tại đã có trong đường đi -> phát hiện vòng lặp
                    if (!path.Add(currentId)) {
                        // Phá vỡ vòng lặp bằng cách biến node BẮT ĐẦU của chuỗi này thành node gốc.
                        setParentId(startNode, default);
                        setPath(startNode, $"/{getId(startNode)}/");
                        setLevel(startNode, 0);
                        break; // Dừng việc đi ngược của chuỗi này
                    }

                    var parentId = getParentId(currentNode);
                    // Nếu không có cha hoặc cha không tồn tại trong danh sách, dừng lại
                    if (parentId == null || !nodeMap.TryGetValue(parentId, out var parentNode)) {
                        break;
                    }
                    currentNode = parentNode;
                }
            }

            // Các bước xử lý còn lại giữ nguyên như cũ, chúng sẽ hoạt động trên dữ liệu đã được làm sạch

            // Kiểm tra parent đã xoá hoặc không có trong danh sách thì set parentId = null (Orphan check)
            var isDeletedProperty = GetCachedIsDeletedProperty();
            foreach (var node in allNodes) {
                var pid = getParentId(node);
                if (pid != null && !pid.Equals(default(TKey))) {
                    // Parent không tồn tại trong danh sách hoặc đã bị soft-delete
                    if (!nodeMap.TryGetValue(pid, out var parent) || (isDeletedProperty != null && (bool)isDeletedProperty.GetValue(parent)!)) {
                        setParentId(node, default);
                    }
                }
            }

            // Tạo map con-theo-cha để tra cứu nhanh, chỉ chứa các key hợp lệ
            var childrenMap = allNodes
                .Where(n => {
                    var pid = getParentId(n);
                    return pid != null && !pid.Equals(default(TKey));
                })
                .GroupBy(n => getParentId(n)!)
                .ToDictionary(g => g.Key, g => g.ToList());

            var visitedNodes = new HashSet<TKey>();

            // Chỉ bắt đầu duyệt từ các node gốc thực sự (sau khi đã dọn dẹp)
            var allNodeIds = new HashSet<TKey>(allNodes.Select(getId));
            var roots = allNodes.Where(n => {
                var pid = getParentId(n);
                return pid == null
                       || pid.Equals(default(TKey))
                       || !allNodeIds.Contains(pid); // ParentId trỏ đến một node không có trong danh sách
            }).ToList();

            // Duyệt qua các node gốc đã xác định
            foreach (var root in roots) {
                if (!visitedNodes.Contains(getId(root))) {
                    DfsSetPath(root, "", -1, childrenMap, visitedNodes, getId, setPath, setLevel);
                }
            }
        }
        #endregion

        // Đặt trong #region MaterializedPath Operations

        /// <summary>
        /// Chuẩn bị Path và Level cho một node MỚI dựa vào thông tin của cha.
        /// Phương thức này không thực hiện bất kỳ lệnh gọi DB nào.
        /// </summary>
        public void InitializeNode(TEntity entity, TEntity? parent) {
            IMaterializedPathEntity<TKey> node = (IMaterializedPathEntity<TKey>)entity;

            // Validate đơn giản cho trường hợp thêm mới
            if (parent != null)
                ManagedException.ThrowIf(entity.Id.Equals(parent.Id), ErrorMessageConstants.SelfReference);

            if (parent != null) {
                IMaterializedPathEntity<TKey> parentNode = (IMaterializedPathEntity<TKey>)parent;
                var parentPath = parentNode.Path ?? "/";
                if (!parentPath.EndsWith('/')) {
                    parentPath += "/";
                }
                var newPath = $"{parentPath}{node.Id}/";
                node.Path = Regex.Replace(newPath, "/{2,}", "/");
                node.Level = parentNode.Level + 1;
            } else {
                node.Path = $"/{node.Id}/";
                node.Level = 0;
            }
        }

        public bool IsMaterializedPath() => typeof(IMaterializedPathEntity<TKey>).IsAssignableFrom(typeof(TEntity));
        // Trong Repository.cs

        /// <summary>
        /// Di chuyển một node tới một vị trí cha mới và cập nhật lại cây con của nó.
        /// </summary>
        public async Task MoveNodeAsync(TEntity entity, TEntity? parent, CancellationToken cancellationToken = default) {
            // 1. Kiểm tra tham chiếu vòng lặp
            ValidateReference(entity, parent);
            IMaterializedPathEntity<TKey> node = (IMaterializedPathEntity<TKey>)entity;

            var oldPath = node.Path ?? "/";
            string newPath;
            int newLevel;

            // 2. Tính toán Path và Level mới
            if (parent != null) {
                IMaterializedPathEntity<TKey> parentNode = (IMaterializedPathEntity<TKey>)parent;
                var parentPath = parentNode.Path ?? "/";
                if (!parentPath.EndsWith('/')) {
                    parentPath += "/";
                }
                newPath = $"{parentPath}{node.Id}/";
                newPath = Regex.Replace(newPath, "/{2,}", "/");

                newLevel = parentNode.Level + 1;
            } else {
                newPath = $"/{node.Id}/";
                newLevel = 0;
            }

            // Nếu không có gì thay đổi thì không cần làm gì cả
            if (oldPath == newPath) {
                return;
            }

            // ... Phần còn lại của phương thức giữ nguyên ...

            // 3. Lấy tất cả con cháu để cập nhật
            var children = await DbSet
                .Cast<IMaterializedPathEntity<TKey>>()
                .Where(e => e.Path!.StartsWith(oldPath) && !e.Id.Equals(node.Id))
                .ToListAsync(cancellationToken);

            var levelDiff = newLevel - node.Level;

            // 4. Cập nhật node hiện tại
            node.Path = newPath;
            node.Level = newLevel;
            node.ParentId = parent == null ? default : parent.Id;

            // 5. Cập nhật con cháu
            foreach (var child in children) {
                child.Path = newPath + child.Path!.Substring(oldPath.Length);
                child.Level += levelDiff;
            }

            DbSet.Update(entity);
        }

        // Sửa lại ValidateReference
        private static void ValidateReference(TEntity entity, TEntity? parent) {
            if (parent == null) return;

            // ✅ 1. Kiểm tra tham chiếu tới chính nó (Self-Reference)
            ManagedException.ThrowIf(entity.Id.Equals(parent.Id), ErrorMessageConstants.SelfReference);

            IMaterializedPathEntity<TKey> node = (IMaterializedPathEntity<TKey>)entity;
            IMaterializedPathEntity<TKey> parentNode = (IMaterializedPathEntity<TKey>)parent;

            var nodePath = node.Path;
            var newParentPath = parentNode.Path;

            // ✅ 2. Kiểm tra tham chiếu vòng (Circular Reference)
            if (!string.IsNullOrEmpty(nodePath) && !string.IsNullOrEmpty(newParentPath) && newParentPath.StartsWith(nodePath)) {
                ManagedException.Throw(ErrorMessageConstants.CircleReference);
            }
        }

        #region Private Helpers
        /// <summary>
        /// Cache PropertyInfo cho Used để tránh reflection mỗi lần gọi
        /// </summary>
        private static PropertyInfo? GetCacheUsedProperty() {
            return _usedPropertyCache.GetOrAdd(typeof(TEntity), type => {
                var prop = type.GetProperty(EntityPropertyNames.Used);
                return prop?.PropertyType == typeof(bool) ? prop : null;
            });
        }

        /// <summary>
        /// Cache PropertyInfo cho IsDeleted để tránh reflection mỗi lần gọi
        /// </summary>
        private static PropertyInfo? GetCachedIsDeletedProperty() {
            return _isDeletedPropertyCache.GetOrAdd(typeof(TEntity), type => {
                var prop = type.GetProperty(EntityPropertyNames.IsDeleted);
                return prop?.PropertyType == typeof(bool) ? prop : null;
            });
        }

        /// <summary>
        /// Cache MethodInfo cho Where method để tránh reflection mỗi lần gọi
        /// </summary>
        private static MethodInfo GetCachedWhereMethod() {
            var cacheKey = $"Where_{typeof(TEntity).Name}";
            return _queryableMethodCache.GetOrAdd(cacheKey, _ =>
                typeof(Queryable).GetMethods()
                    .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(TEntity)));
        }

        /// <summary>
        /// Cache MethodInfo cho OrderByDescending method để tránh reflection mỗi lần gọi
        /// </summary>
        private static MethodInfo GetCachedOrderByDescendingMethod() {
            var cacheKey = $"OrderByDescending_{typeof(TEntity).Name}_long";
            return _queryableMethodCache.GetOrAdd(cacheKey, _ =>
                typeof(Queryable).GetMethods()
                    .First(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(TEntity), typeof(long)));
        }

        /// <summary>
        /// Cache interface implementation check để tránh reflection mỗi lần gọi
        /// </summary>
        private static bool IsImplementsInterface(Type entityType, Type interfaceType) {
            var cacheKey = $"{entityType.Name}_{interfaceType.Name}";
            return _implementsInterfaceCache.GetOrAdd(cacheKey, _ => interfaceType.IsAssignableFrom(entityType));
        }

        /// <summary>
        /// Cache PropertyInfo cho Path property để tránh reflection mỗi lần gọi
        /// ✅ Static method for better performance and thread safety
        /// </summary>
        private static PropertyInfo? GetCachedPathProperty(Type entityType) {
            return _pathPropertyCache.GetOrAdd(entityType, type => type.GetProperty(EntityPropertyNames.Path));
        }

        /// <summary>
        /// Cache PropertyInfo cho Level property để tránh reflection mỗi lần gọi
        /// ✅ Static method for better performance and thread safety
        /// </summary>
        private static PropertyInfo? GetCachedLevelProperty(Type entityType) {
            return _levelPropertyCache.GetOrAdd(entityType, type => type.GetProperty(EntityPropertyNames.Level));
        }

        // Hàm đệ quy duyệt sâu
        private static void DfsSetPath(
            TEntity currentNode,
            string parentPath,
            int parentLevel,
            Dictionary<TKey, List<TEntity>> children,
            HashSet<TKey> visited,
            Func<TEntity, TKey> getId,
            Action<TEntity, string> setPath,
            Action<TEntity, int> setLevel
        ) {
            var currentId = getId(currentNode);

            // An toàn: nếu node đã được thăm (do cấu trúc dữ liệu phức tạp), không xử lý lại
            if (visited.Contains(currentId)) {
                return;
            }
            visited.Add(currentId);

            // Tính toán path & level chính xác
            // Path của cha đã có dấu "/", nên chỉ cần nối thêm ID
            var newPath = $"{parentPath}/{currentId}/";
            newPath = Regex.Replace(newPath, "/{2,}", "/");
            var newLevel = parentLevel + 1;

            setPath(currentNode, newPath);
            setLevel(currentNode, newLevel);

            // Đệ quy: Duyệt các node con
            if (children.TryGetValue(currentId, out var childNodes)) {
                foreach (var child in childNodes) {
                    // Thêm kiểm tra an toàn trước khi đệ quy để chống StackOverflow
                    if (!visited.Contains(getId(child))) {
                        DfsSetPath(child, newPath, newLevel, children, visited, getId, setPath, setLevel);
                    }
                }
            }
        }
        #endregion
    }
}
