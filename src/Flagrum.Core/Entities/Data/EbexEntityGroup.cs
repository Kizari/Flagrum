using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class EntityGroup : Entity
{
    public enum FindEntityPackageMethod
    {
        None,
        DepthFirst,
        BreathFirst,
        FollowParent
    }

    public new const string ClassFullName = "SQEX.Ebony.Framework.Entity.EntityGroup";

    public EntityGroup(DataItem parent)
        : base(parent, "SQEX.Ebony.Framework.Entity.EntityGroup") { }

    public EntityGroup(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public DynamicArray Entities => this["entities_"] as DynamicArray;

    public SubPackageCollection SubPackagesExceptSubEntityGroup => new(this);

    public bool HasTransform => GetBool("hasTransform_");

    public bool CanManipulate => GetBool("canManipulate_");

    public void ClearEntities()
    {
        try
        {
            if (!(this["entities_"] is DynamicArray dynamicArray2))
            {
                return;
            }

            dynamicArray2.ClearChild();
        }
        catch (Exception ex)
        {
            // DocumentInterface.DocumentContainer.LogAppendCurrentParagraphParse(
            //     "<Span Foreground=\"Red\">ClearEntities : " + ex.Message + "</Span><LineBreak />");
        }
    }

    public void UpdateAllIndexAtLoading()
    {
        foreach (var entity in Entities)
        {
            if (entity is SequenceContainer)
            {
                ((SequenceContainer)entity).UpdateAllIndexAtLoading();
            }

            if (entity is EntityGroup && !(entity is EntityPackage) && !(entity is Prefab))
            {
                ((EntityGroup)entity).UpdateAllIndexAtLoading();
            }
        }
    }

    public void SortEntitesByName()
    {
        Entities.Children.Sort((item0, item1) => item0.Name.CompareTo(item1.Name));
        Modified = true;
    }

    public void AccumulateEntities(List<Entity> result, bool packageRecursive)
    {
        if (Entities == null)
        {
            Trace.WriteLine("Entities is null");
        }
        else
        {
            foreach (var entity in Entities.OfType<Entity>())
            {
                result.Add(entity);
                if (entity is EntityGroup)
                {
                    var entityGroup = entity as EntityGroup;
                    if (!(entityGroup is EntityPackage) || packageRecursive)
                    {
                        entityGroup.AccumulateEntities(result, packageRecursive);
                    }
                }
            }
        }
    }

    public void RetrieveEntitiesWithFunc(
        List<Entity> result,
        bool packageRecursive,
        Func<Entity, bool> judgeFunc)
    {
        if (Entities == null)
        {
            Trace.WriteLine("Entities is null");
        }
        else
        {
            foreach (var entity in Entities.OfType<Entity>())
            {
                if (judgeFunc(entity))
                {
                    result.Add(entity);
                }

                if (entity is EntityGroup)
                {
                    var entityGroup = entity as EntityGroup;
                    if (!(entityGroup is EntityPackage) || packageRecursive)
                    {
                        entityGroup.RetrieveEntitiesWithFunc(result, packageRecursive, judgeFunc);
                    }
                }
            }
        }
    }

    public void RetrieveEntitiesWithFunc(
        List<Entity> result,
        Func<Entity, bool> judgeFunc,
        Func<EntityGroup, bool> recurseTerminateFunc)
    {
        if (Entities == null)
        {
            Trace.WriteLine("Entities is null");
        }
        else
        {
            foreach (var entity in Entities.OfType<Entity>())
            {
                if (judgeFunc(entity))
                {
                    result.Add(entity);
                }

                if (entity is EntityGroup)
                {
                    var entityGroup = entity as EntityGroup;
                    if (!recurseTerminateFunc(entityGroup))
                    {
                        entityGroup.RetrieveEntitiesWithFunc(result, judgeFunc, recurseTerminateFunc);
                    }
                }
            }
        }
    }

    public void AccumulateDataItems(List<DataItem> result, bool packageRecursive)
    {
        if (Entities == null)
        {
            Trace.WriteLine("Entities is null");
        }
        else
        {
            foreach (var entity in Entities.OfType<Entity>())
            {
                result.Add(entity);
                if (entity is SequenceContainer)
                {
                    result.AddRange(((SequenceContainer)entity).AllItemsRecursive);
                }

                if (entity is EntityGroup)
                {
                    var entityGroup = entity as EntityGroup;
                    if (!(entityGroup is EntityPackage) || packageRecursive)
                    {
                        entityGroup.AccumulateDataItems(result, packageRecursive);
                    }
                }
            }
        }
    }

    public void AccumulateEntityPackages(
        List<EntityPackage> result,
        bool packageRecursive,
        bool withoutPrefab = true)
    {
        if (Entities == null)
        {
            Trace.WriteLine("Entities is null");
        }
        else
        {
            foreach (var entity in Entities.OfType<Entity>())
            {
                if (entity is EntityPackage entityPackage3)
                {
                    if (withoutPrefab)
                    {
                        if (!(entityPackage3 is Prefab))
                        {
                            result.Add(entityPackage3);
                        }
                    }
                    else
                    {
                        result.Add(entityPackage3);
                    }
                }

                if (entity is EntityGroup)
                {
                    var entityGroup = entity as EntityGroup;
                    if (!(entityGroup is EntityPackage) || packageRecursive)
                    {
                        entityGroup.AccumulateEntityPackages(result, packageRecursive, withoutPrefab);
                    }
                }
            }
        }
    }

    public void RetrieveEntityPackagesWithFunc(
        List<EntityPackage> result,
        bool packageRecursive,
        bool recurceAtJudgeFalse,
        Func<EntityPackage, bool> judgeFunc)
    {
        if (Entities == null)
        {
            Trace.WriteLine("Entities is null");
        }
        else
        {
            foreach (var entity in Entities.OfType<Entity>())
            {
                if (entity is EntityPackage entityPackage3)
                {
                    if (judgeFunc(entityPackage3))
                    {
                        result.Add(entityPackage3);
                    }
                    else if (!recurceAtJudgeFalse)
                    {
                        continue;
                    }
                }

                if (entity is EntityGroup)
                {
                    var entityGroup = entity as EntityGroup;
                    if (!(entityGroup is EntityPackage) || packageRecursive)
                    {
                        entityGroup.RetrieveEntityPackagesWithFunc(result, packageRecursive, recurceAtJudgeFalse,
                            judgeFunc);
                    }
                }
            }
        }
    }

    public void AccumulateEntitiesWithDataType(
        List<DataItem> result,
        DataType dataType,
        bool packageRecursive)
    {
        if (Entities == null)
        {
            Trace.WriteLine("Entities is null");
        }
        else
        {
            foreach (var entity in Entities.OfType<Entity>())
            {
                if (entity.DataType == dataType)
                {
                    result.Add(entity);
                }

                if (entity is EntityGroup)
                {
                    var entityGroup = entity as EntityGroup;
                    if (!(entityGroup is EntityPackage) || packageRecursive)
                    {
                        entityGroup.AccumulateEntitiesWithDataType(result, dataType, packageRecursive);
                    }
                }
            }
        }
    }

    public void AccumulateEntitiesWithTypeFullName(
        List<DataItem> result,
        string typeFullName,
        bool packageRecursive)
    {
        if (Entities == null)
        {
            Trace.WriteLine("Entities is null");
        }
        else
        {
            foreach (var entity in Entities.OfType<Entity>())
            {
                if (entity.DataType.FullName == typeFullName)
                {
                    result.Add(entity);
                }

                if (entity is EntityGroup)
                {
                    var entityGroup = entity as EntityGroup;
                    if (!(entityGroup is EntityPackage) || packageRecursive)
                    {
                        entityGroup.AccumulateEntitiesWithTypeFullName(result, typeFullName, packageRecursive);
                    }
                }
            }
        }
    }

    public EntityPackage FindEntityPackageWithSourcePath(
        string i_sourceFullPath,
        bool isAllowLazyLoading,
        FindEntityPackageMethod method = FindEntityPackageMethod.DepthFirst)
    {
        i_sourceFullPath.Replace('\\', '/');
        switch (method)
        {
            case FindEntityPackageMethod.DepthFirst:
                return findEntityPackageWithSourcePathDFS(i_sourceFullPath, isAllowLazyLoading);
            case FindEntityPackageMethod.BreathFirst:
                return findEntityPackageWithSourcePathBFS(i_sourceFullPath, isAllowLazyLoading);
            case FindEntityPackageMethod.FollowParent:
                return findEntityPackageWithSourcePathFollowParent(i_sourceFullPath, isAllowLazyLoading);
            default:
                return null;
        }
    }

    private EntityPackage findEntityPackageWithSourcePathDFS(
        string i_sourceFullPath,
        bool isAllowLazyLoading)
    {
        var i_sourceFullPath1 = i_sourceFullPath.Replace('\\', '/');
        if (this is EntityPackage entityPackage)
        {
            if (entityPackage.FullFilePath.Replace('\\', '/') == i_sourceFullPath1)
            {
                return entityPackage;
            }

            if (!entityPackage.IsLoaded & isAllowLazyLoading && DocumentInterface.DocumentContainer != null)
            {
                DocumentInterface.DocumentContainer.LazyLoading(entityPackage);
            }
        }

        foreach (var entityGroup in Entities.OfType<EntityGroup>())
        {
            var withSourcePathDfs =
                entityGroup.findEntityPackageWithSourcePathDFS(i_sourceFullPath1, isAllowLazyLoading);
            if (withSourcePathDfs != null)
            {
                return withSourcePathDfs;
            }
        }

        return null;
    }

    private EntityPackage findEntityPackageWithSourcePathBFS(
        string i_sourceFullPath,
        bool isAllowLazyLoading)
    {
        var str = i_sourceFullPath.Replace('\\', '/');
        var dataItemQueue = new Queue<DataItem>();
        dataItemQueue.Enqueue(this);
        while (dataItemQueue.Count != 0)
        {
            var dataItem = dataItemQueue.Dequeue();
            if (dataItem is EntityPackage entityPackage2)
            {
                if (entityPackage2.FullFilePath.Replace('\\', '/') == str)
                {
                    return entityPackage2;
                }

                if (!entityPackage2.IsLoaded & isAllowLazyLoading && DocumentInterface.DocumentContainer != null)
                {
                    DocumentInterface.DocumentContainer.LazyLoading(entityPackage2);
                }
            }

            if (dataItem is EntityGroup entityGroup2 && entityGroup2.Entities != null)
            {
                foreach (var entity in entityGroup2.Entities)
                {
                    if (entity.Parent != null && entity.Parent.Parent == dataItem)
                    {
                        dataItemQueue.Enqueue(entity);
                    }
                }
            }
        }

        return null;
    }

    private EntityPackage findEntityPackageWithSourcePathFollowParent(
        string i_sourceFullPath,
        bool isAllowLazyLoading)
    {
        var str = i_sourceFullPath.Replace('\\', '/');
        if (this is EntityPackage entityPackage)
        {
            if (entityPackage.FullFilePath.Replace('\\', '/') == str)
            {
                return entityPackage;
            }

            if (!entityPackage.IsLoaded & isAllowLazyLoading && DocumentInterface.DocumentContainer != null)
            {
                DocumentInterface.DocumentContainer.LazyLoading(entityPackage);
            }

            if (entityPackage.IsRootPackage)
            {
                return null;
            }
        }

        return ParentPackage != null
            ? ParentPackage.findEntityPackageWithSourcePathFollowParent(i_sourceFullPath, isAllowLazyLoading)
            : null;
    }

    public bool ContainsAsSubPackage(
        EntityPackage target,
        bool packageRecursive,
        bool checkStartupTrue = true)
    {
        foreach (var entity in Entities.OfType<Entity>())
        {
            if (entity.IsChecked && entity is EntityGroup entityGroup1)
            {
                if (entity is EntityPackage entityPackage3)
                {
                    if (!checkStartupTrue || !(entityPackage3 is LoadUnitPackage loadUnitPackage) ||
                        loadUnitPackage.StartupLoad)
                    {
                        if (entityPackage3 == target)
                        {
                            return true;
                        }

                        if (!packageRecursive)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                if (entityGroup1.ContainsAsSubPackage(target, packageRecursive, checkStartupTrue))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override float[] GetLocalPosition()
    {
        if (GetBool("hasTransform_"))
        {
            return base.GetLocalPosition();
        }

        return new float[4] {0.0f, 0.0f, 0.0f, 1f};
    }

    public override float[] GetLocalRotationDegree()
    {
        return GetBool("hasTransform_") ? base.GetLocalRotationDegree() : new float[4];
    }

    public float[] CalculateLocalPositionFromThis(float[] worldPosition)
    {
        throw new NotImplementedException();
        // var matrix = Matrix.Identity;
        // if (!(this is WorldPackage))
        // {
        //     matrix = Matrix.OrthoInverse(getWorldRTPose());
        // }
        //
        // var identity = Matrix.Identity;
        // identity.SetCol(3, new Vector(worldPosition[0], worldPosition[1], worldPosition[2], 1f));
        // var col3 = (matrix * identity).Col3;
        // return new float[4]
        // {
        //     col3[0],
        //     col3[1],
        //     col3[2],
        //     1f
        // };
    }

    public float[] CalculateLocalRotationFromThis(float[] worldRotation)
    {
        throw new NotImplementedException();
        // var matrix = Matrix.Identity;
        // if (!(this is WorldPackage))
        // {
        //     matrix = Matrix.OrthoInverse(getWorldRTPose());
        // }
        //
        // var rotationZyx =
        //     Matrix.GetRotationZYX(new Vector(worldRotation[0], worldRotation[1], worldRotation[2], 1f));
        // var eulerAngleRadian = Matrix.GetEulerAngleRadian(matrix * rotationZyx);
        // return new float[4]
        // {
        //     eulerAngleRadian[0],
        //     eulerAngleRadian[1],
        //     eulerAngleRadian[2],
        //     1f
        // };
    }

    // public double[] CalculateWorldPositionFromThis(float[] localPosition)
    // {
    //     var matrix = Matrix.Identity;
    //     if (!(this is WorldPackage))
    //     {
    //         matrix = getWorldRTPose();
    //     }
    //
    //     var identity = Matrix.Identity;
    //     identity.SetCol(3, new Vector(localPosition[0], localPosition[1], localPosition[2], 1f));
    //     var col3 = (matrix * identity).Col3;
    //     return new double[4]
    //     {
    //         col3[0],
    //         col3[1],
    //         col3[2],
    //         1.0
    //     };
    // }

    public void SetPivotToCenter()
    {
        var pivotWorldPosition = new float[3];
        var num = 0;
        foreach (var entity in Entities)
        {
            if (entity is Entity)
            {
                var worldPosition = ((Entity)entity).GetWorldPosition();
                if (worldPosition != null)
                {
                    for (var index = 0; index < 3; ++index)
                    {
                        pivotWorldPosition[index] += worldPosition[index];
                    }

                    ++num;
                }
            }
        }

        if (num > 0)
        {
            for (var index = 0; index < 3; ++index)
            {
                pivotWorldPosition[index] = pivotWorldPosition[index] / num;
            }
        }

        SetPivot(pivotWorldPosition);
    }

    public void SetPivot(float[] pivotWorldPosition)
    {
        throw new NotImplementedException();
        // var worldPosition = GetWorldPosition();
        // var numArray = new float[3];
        // for (var index = 0; index < 3; ++index)
        // {
        //     numArray[index] = pivotWorldPosition[index] - worldPosition[index];
        // }
        //
        // var localRotationDegree = GetLocalRotationDegree();
        // var localScaling = GetLocalScaling();
        // var matrix1 = new Matrix(new Vector(localScaling, 0, 0), new Vector(0, localScaling, 0),
        //     new Vector(0, 0, localScaling), Vector.Zero);
        // var matrix2 = Matrix.OrthoInverse(Matrix.GetRotationZYX(new Vector(localRotationDegree[0] * Math.PI / 180.0,
        //     localRotationDegree[1] * Math.PI / 180.0, localRotationDegree[2] * Math.PI / 180.0, 0.0f)));
        // var vector = (matrix2.Col0 * numArray[0] + matrix2.Col1 * numArray[1] + matrix2.Col2 * numArray[2]) /
        //              localScaling;
        // for (var index1 = 0; index1 < Entities.Count; ++index1)
        // {
        //     if (Entities[index1] is Entity entity1)
        //     {
        //         var localPosition = entity1.GetLocalPosition();
        //         if (localPosition != null)
        //         {
        //             for (var index2 = 0; index2 < 3; ++index2)
        //             {
        //                 localPosition[index2] -= vector[index2];
        //             }
        //
        //             entity1.SetLocalPosition(localPosition);
        //         }
        //     }
        // }
        //
        // SetLocalPositionFromWorldPosition(pivotWorldPosition);
    }

    public class SubPackageCollection : IEnumerable<EntityPackage>, IEnumerable
    {
        private readonly EntityGroup host_;

        public SubPackageCollection(EntityGroup host)
        {
            host_ = host;
        }

        public IEnumerator<EntityPackage> GetEnumerator()
        {
            return new SubPackageEnumerator(host_);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SubPackageEnumerator(host_);
        }
    }

    public class SubPackageEnumerator : IEnumerator<EntityPackage>, IDisposable, IEnumerator
    {
        private readonly EntityGroup host_;
        private int index_;

        public SubPackageEnumerator(EntityGroup host)
        {
            host_ = host;
            index_ = -1;
        }

        public EntityPackage Current => (EntityPackage)host_.Entities[index_];

        public void Dispose() { }

        object IEnumerator.Current => host_.Entities[index_];

        public bool MoveNext()
        {
            var entities = host_.Entities;
            var count = entities.Count;
            for (var index = index_ + 1; index < count; ++index)
            {
                if (entities[index] is EntityPackage)
                {
                    index_ = index;
                    return true;
                }
            }

            index_ = count;
            return false;
        }

        public void Reset()
        {
            index_ = -1;
        }
    }
}