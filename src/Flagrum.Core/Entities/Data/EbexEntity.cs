using System.Collections.Generic;
using Flagrum.Core.Scripting.Ebex.Math;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class Entity : Object
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Entity.Entity";
    public const string DIFFERENCES_PARAM_NAME = "differences";
    public const string DIFFGROUP_PATH_PARAM_NAME = "diffGroupPath";
    private readonly List<DataItem> referencedItems_ = new();

    public Entity(DataItem parent, string typeFullName)
        : base(parent, typeFullName)
    {
        createCommentItem();
    }

    public Entity(DataItem parent, DataType dataType)
        : base(parent, dataType)
    {
        createCommentItem();
    }

    public override void Dispose()
    {
        if (DocumentInterface.DocumentContainer != null && !DocumentInterface.DocumentContainer.IsSettingNewScene)
        {
            foreach (var referencedItem in referencedItems_)
            {
                if (referencedItem.Value is Value obj2 && obj2.PrimitiveType == PrimitiveType.Pointer)
                {
                    if (obj2.Object as Entity == this)
                    {
                        obj2.Object = null;
                    }
                    // DocumentInterface.DocumentContainer.PrintWarning(
                    //     "Entity::Dispose() : referencedItem error : " + referencedItem.Name);
                }
            }
        }

        base.Dispose();
    }

    public List<DataItem> GetReferencedItemList()
    {
        return referencedItems_;
    }

    public void AddReferencedItem(DataItem item)
    {
        if (referencedItems_.Contains(item))
        {
            return;
        }

        referencedItems_.Add(item);
    }

    public void RemoveReferencedItem(DataItem item)
    {
        referencedItems_.Remove(item);
    }

    public void ModifyReferencedItem()
    {
        foreach (var referencedItem in referencedItems_)
        {
            referencedItem.Modified = true;
        }
    }

    public virtual float[] GetLocalPosition()
    {
        var float4 = GetFloat4("position_");
        if (float4 == null)
        {
            return null;
        }

        return new float[4]
        {
            float4[0],
            float4[1],
            float4[2],
            float4[3]
        };
    }

    public virtual float[] GetLocalRotationDegree()
    {
        var float4 = GetFloat4("rotation_");
        if (float4 == null)
        {
            return null;
        }

        return new float[4]
        {
            float4[0],
            float4[1],
            float4[2],
            float4[3]
        };
    }

    public virtual float GetLocalScaling()
    {
        var num = GetFloat("scaling_");
        return num <= 0.0 ? 1f : num;
    }

    public void SetLocalPosition(float[] localPosition)
    {
        SetLocalPosition(new Value(localPosition[0], localPosition[1], localPosition[2], 1f));
    }

    public void SetLocalPosition(Value positionValue)
    {
        var dataItem = this["position_"];
        if (dataItem == null ||
            CheckEqualF4(positionValue.GetFloatArray(), ((Value)dataItem.Value).GetFloatArray()))
        {
            return;
        }

        dataItem.Value = positionValue;
        Modified = true;
    }

    public void SetLocalRotationDegree(float[] localRotationDegree)
    {
        var dataItem = this["rotation_"];
        if (dataItem == null || localRotationDegree.Length < 3)
        {
            return;
        }

        var floatArray = ((Value)dataItem.Value).GetFloatArray();
        if (CheckEqualF4(localRotationDegree, floatArray))
        {
            return;
        }

        dataItem.Value = new Value(localRotationDegree[0], localRotationDegree[1], localRotationDegree[2], 0.0f);
        Modified = true;
    }

    public virtual void SetLocalScaling(float scale)
    {
        SetFloat("scaling_", scale);
        Modified = true;
    }

    public void SetLocalPositionFromWorldPosition(float[] worldPosition)
    {
        var dataItem = this["position_"];
        if (dataItem == null)
        {
            return;
        }

        if (ParentGroup != null)
        {
            var positionFromThis = ParentGroup.CalculateLocalPositionFromThis(worldPosition);
            positionFromThis[3] = 1f;
            var floatArray = ((Value)dataItem.Value).GetFloatArray();
            if (CheckEqualF4(positionFromThis, floatArray))
            {
                return;
            }

            dataItem.Value = new Value(positionFromThis[0], positionFromThis[1], positionFromThis[2], 1f);
            Modified = true;
        }
        else
        {
            var fArray1 = new float[4]
            {
                worldPosition[0],
                worldPosition[1],
                worldPosition[2],
                1f
            };
            var floatArray = ((Value)dataItem.Value).GetFloatArray();
            if (CheckEqualF4(fArray1, floatArray))
            {
                return;
            }

            dataItem.Value = new Value(fArray1[0], fArray1[1], fArray1[2], fArray1[3]);
            Modified = true;
        }
    }

    public void SetLocalRotationFromWorldRotation(float[] worldRotation)
    {
        var dataItem = this["rotation_"];
        if (dataItem == null)
        {
            return;
        }

        if (ParentGroup != null)
        {
            var rotationFromThis = ParentGroup.CalculateLocalRotationFromThis(worldRotation);
            rotationFromThis[3] = 1f;
            var floatArray = ((Value)dataItem.Value).GetFloatArray();
            if (CheckEqualF4(rotationFromThis, floatArray))
            {
                return;
            }

            dataItem.Value = new Value((float)(rotationFromThis[0] * 180.0 / System.Math.PI),
                (float)(rotationFromThis[1] * 180.0 / System.Math.PI), (float)(rotationFromThis[2] * 180.0 / System.Math.PI), 1f);
            Modified = true;
        }
        else
        {
            var fArray1 = new float[4]
            {
                worldRotation[0],
                worldRotation[1],
                worldRotation[2],
                1f
            };
            var floatArray = ((Value)dataItem.Value).GetFloatArray();
            if (CheckEqualF4(fArray1, floatArray))
            {
                return;
            }

            dataItem.Value = new Value(fArray1[0], fArray1[1], fArray1[2], fArray1[3]);
            Modified = true;
        }
    }

    public static bool CheckEqualF4(float[] fArray1, float[] fArray2, bool checkWithEpsilon = false)
    {
        var vector1 = new Vector(fArray1[0], fArray1[1], fArray1[2]);
        var vector2 = new Vector(fArray2[0], fArray2[1], fArray2[2]);
        if (checkWithEpsilon)
        {
            var num = 1E-05f;
            return System.Math.Abs(fArray1[0] - fArray2[0]) <= (double)num &&
                   System.Math.Abs(fArray1[1] - fArray2[1]) <= (double)num &&
                   System.Math.Abs(fArray1[2] - fArray2[2]) <= (double)num;
        }

        return vector1.Equals(vector2);
    }

    public static bool CheckEqual3(double[] fArray1, double[] fArray2, bool checkWithEpsilon = false)
    {
        if (checkWithEpsilon)
        {
            var num = 9.99999974737875E-06;
            return System.Math.Abs(fArray1[0] - fArray2[0]) <= num && System.Math.Abs(fArray1[1] - fArray2[1]) <= num &&
                   System.Math.Abs(fArray1[2] - fArray2[2]) <= num;
        }

        return new Vector(fArray1[0], fArray1[1], fArray1[2]).Equals(new Vector(fArray2[0], fArray2[1],
            fArray2[2]));
    }

    public float[] GetWorldPosition()
    {
        if (GetLocalPosition() == null || GetLocalRotationDegree() == null)
        {
            return null;
        }

        var col3 = getWorldRTPose().Col3;
        return new float[4]
        {
            col3.X,
            col3.Y,
            col3.Z,
            1f
        };
    }

    public double[] GetDoubleWorldPosition()
    {
        if (GetLocalPosition() == null || GetLocalRotationDegree() == null)
        {
            return null;
        }

        var col3 = getWorldRTPose().Col3;
        return new double[4]
        {
            col3.X,
            col3.Y,
            col3.Z,
            1.0
        };
    }

    public float[] GetWorldRotation()
    {
        if (GetLocalPosition() == null || GetLocalRotationDegree() == null)
        {
            return null;
        }

        var eulerAngleRadian = Matrix.GetEulerAngleRadian(getWorldRTPose());
        return new float[4]
        {
            eulerAngleRadian.X,
            eulerAngleRadian.Y,
            eulerAngleRadian.Z,
            1f
        };
    }

    public float GetWorldScaling()
    {
        return ParentGroup == null ? GetLocalScaling() : ParentGroup.GetWorldScaling() * GetLocalScaling();
    }

    protected Matrix getWorldRTPose()
    {
        var localPosition = GetLocalPosition();
        var localRotationDegree = GetLocalRotationDegree();
        var vector1 = new Vector(localPosition[0], localPosition[1], localPosition[2]);
        var eulerRadians1 = new Vector(localRotationDegree[0] * System.Math.PI / 180.0,
            localRotationDegree[1] * System.Math.PI / 180.0, localRotationDegree[2] * System.Math.PI / 180.0, 0.0f);
        Vector vector2;
        Vector eulerRadians2;
        if (ParentGroup == null || ParentGroup is WorldPackage)
        {
            vector2 = vector1;
            eulerRadians2 = eulerRadians1;
        }
        else
        {
            var worldRtPose = ParentGroup.getWorldRTPose();
            vector2 = worldRtPose.TransformCoord(vector1 * ParentGroup.GetWorldScaling());
            eulerRadians2 = Matrix.GetEulerAngleRadian(worldRtPose * Matrix.GetRotationZYX(eulerRadians1));
        }

        var rotationZyx = Matrix.GetRotationZYX(eulerRadians2);
        rotationZyx.SetCol(3, vector2);
        return rotationZyx;
    }

    public bool AddDifference(DataItem dataItem, bool forceAdd = false)
    {
        if (dataItem == this || dataItem.Parent is Prefab || dataItem.Parent.Name == "entities_")
        {
            return false;
        }

        if (!forceAdd && !Prefab.DifferenceMode)
        {
            var flag = false;
            if (Prefab.CharaEntryPrefabMode && this["differences"] != null)
            {
                flag = true;
            }

            if (!flag)
            {
                return false;
            }
        }

        if (dataItem is DynamicArray)
        {
            return false;
        }

        if (dataItem.Parent.IsDynamic && dataItem.Parent.Parent is DynamicArray)
        {
            var num1 = dataItem.Parent.Parent.Children.IndexOf(dataItem.Parent);
            if (dataItem.Parent.Name != null && dataItem.Parent.Name.Length >= 3)
            {
                var s = dataItem.Parent.Name.Substring(dataItem.Parent.Name.Length - 3);
                if (s[0] < '0' || s[0] > '9')
                {
                    s = s.Substring(1);
                    if (s[0] < '0' || s[0] > '9')
                    {
                        s = s.Substring(1);
                    }
                }

                var result = -1;
                if (int.TryParse(s, out result) && num1 != result)
                {
                    if (DocumentInterface.DocumentContainer != null)
                    {
                        // var num2 = (int)DocumentInterface.DocumentContainer.ShowMessageBox(
                        //     "差分のIndexとNameがずれているので差分は作成しません : indexOfArray=" + num1 + " indexFromStr=" + result +
                        //     "\n（北出までご相談下さい）");
                    }

                    return false;
                }
            }
        }

        var pathFromThisEntity = getRelativeItemPathFromThisEntity(dataItem.Parent);
        if (this["differences"] == null)
        {
            var entityDiff = new EntityDiff(this);
            entityDiff.Name = "differences";
            entityDiff.Field = new Field(entityDiff.Name, "SQEX.Ebony.Std.DynamicArray", false);
            entityDiff.Browsable = false;
            if (DocumentInterface.DocumentContainer != null)
            {
                DocumentInterface.DocumentContainer.doOnEntityPackageModifiedChanged(ParentPackage);
            }
        }

        var flag1 = false;
        var dataItem1 = getDifferenceGroup(pathFromThisEntity, dataItem.Name);
        if (dataItem1 == null)
        {
            flag1 = true;
        }

        if (flag1)
        {
            dataItem1 = new EntityDiffGroup(this["differences"]);
            dataItem1.Name = createDifferenceGroupName();
            dataItem1.Field = new Field(dataItem1.Name, "SQEX.Ebony.Std.DynamicArray", false);
            dataItem1.Browsable = false;
            var valueDataItem = new ValueDataItem(dataItem1, "string");
            valueDataItem.Name = "diffGroupPath";
            valueDataItem.Value = new Value(pathFromThisEntity.ToString());
        }

        if (dataItem1[dataItem.Name] == null)
        {
            var dataItem2 = dataItem.Clone(dataItem1);
            dataItem2.Name = dataItem.Name;
            dataItem2.Value = dataItem.Value;
            if (dataItem.DataType != null)
            {
                dataItem2.Field = dataItem.Field;
            }

            dataItem.IsDefault = false;
        }

        if (dataItem1["Version"] == null)
        {
            var valueDataItem = new ValueDataItem(dataItem1, "string");
            valueDataItem.Name = "Version";
            valueDataItem.Value = new Value("20");
        }

        return true;
    }

    public void ResolveDifference()
    {
        var dataItem1 = this["differences"];
        if (dataItem1 == null)
        {
            return;
        }

        dataItem1.Browsable = false;
        for (var index1 = 0; index1 < dataItem1.Children.Count; ++index1)
        {
            var child1 = dataItem1.Children[index1];
            var dataItem2 = child1["diffGroupPath"];
            if (dataItem2 == null || dataItem2.Value == null)
            {
                dataItem1.Children.RemoveAt(index1);
                --index1;
            }
            else
            {
                var child2 = GetChild(new ItemPath(dataItem2.Value.ToString()));
                if (child2 == null)
                {
                    dataItem1.Children.RemoveAt(index1);
                    --index1;
                }
                else
                {
                    var flag = true;
                    var str = "";
                    for (var index2 = 0; index2 < child1.Children.Count; ++index2)
                    {
                        var child3 = child1.Children[index2];
                        if (!(child3.Name == "diffGroupPath"))
                        {
                            var dataItem3 = child2[child3.Name];
                            if (dataItem3 == null)
                            {
                                if (child3.Name == "Version")
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            else if (dataItem3.Value is Value)
                            {
                                str = child3.Name;
                                switch (((Value)dataItem3.Value).PrimitiveType)
                                {
                                    case PrimitiveType.String:
                                    case PrimitiveType.Fixid:
                                        flag = false;
                                        goto label_17;
                                    default:
                                        continue;
                                }
                            }
                        }
                    }

                    label_17:
                    if (flag)
                    {
                        dataItem1.Children.RemoveAt(index1);
                        --index1;
                        if (DocumentInterface.DocumentContainer != null)
                        {
                            // DocumentInterface.DocumentContainer.PrintWarning("Prefab差分 古いもの削除 : " +
                            //     child1["diffGroupPath"].Value + " : " + str);
                        }
                    }
                    else
                    {
                        for (var index3 = 0; index3 < child1.Children.Count; ++index3)
                        {
                            var child4 = child1.Children[index3];
                            if (!(child4.Name == "diffGroupPath"))
                            {
                                var dataItem4 = child2[child4.Name];
                                if (dataItem4 == null)
                                {
                                    if (!(child4.Name == "Version"))
                                    {
                                        child1.Children.RemoveAt(index3);
                                        --index3;
                                    }
                                }
                                else
                                {
                                    dataItem4.Value = child4.Value;
                                    child4.Field = dataItem4.Field;
                                    if (child4 is DynamicArray)
                                    {
                                        var num = dataItem4.ParentPackageWithoutPrefab.Modified ? 1 : 0;
                                        ((DynamicArray)child4).CloneArrayTo((DynamicArray)dataItem4);
                                        if (num == 0)
                                        {
                                            dataItem4.ParentPackageWithoutPrefab.Modified = false;
                                        }
                                    }

                                    dataItem4.IsDefault = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void ClearDiffernce(DataItem clearedEntity = null)
    {
        var dataItem1 = this["differences"];
        if (dataItem1 == null)
        {
            return;
        }

        for (var index1 = 0; index1 < dataItem1.Children.Count; ++index1)
        {
            var child1 = dataItem1.Children[index1];
            var s = child1["diffGroupPath"].Value.ToString();
            var child2 = GetChild(new ItemPath(s));
            if (clearedEntity != null)
            {
                var str = getRelativeItemPathFromThisEntity(clearedEntity).ToString();
                if (s != str && !s.StartsWith(str + "."))
                {
                    continue;
                }
            }

            if (child2 != null)
            {
                for (var index2 = 0; index2 < child1.Children.Count; ++index2)
                {
                    var child3 = child1.Children[index2];
                    if (!(child3.Name == "diffGroupPath"))
                    {
                        var dataItem2 = child2[child3.Name];
                        if (dataItem2 != null)
                        {
                            dataItem2.IsDefault = true;
                        }

                        child1.Children.RemoveAt(index2);
                        --index2;
                    }
                }
            }

            dataItem1.Children.RemoveAt(index1);
            --index1;
        }

        if (dataItem1.Children.Count != 0)
        {
            return;
        }

        dataItem1.Dispose();
    }

    public void ClearDataItemDiffernce(DataItem clearItem)
    {
        if (clearItem == null || clearItem.Parent == null)
        {
            return;
        }

        var dataItem1 = this["differences"];
        if (dataItem1 == null)
        {
            return;
        }

        for (var index1 = 0; index1 < dataItem1.Children.Count; ++index1)
        {
            var child1 = dataItem1.Children[index1];
            var child2 = GetChild(new ItemPath(child1["diffGroupPath"].Value.ToString()));
            var flag = false;
            if (child2 != null)
            {
                for (var index2 = 0; index2 < child1.Children.Count; ++index2)
                {
                    var child3 = child1.Children[index2];
                    if (!(child3.Name == "diffGroupPath") && !(child3.Name == "Version"))
                    {
                        var dataItem2 = child2[child3.Name];
                        if (dataItem2 == clearItem)
                        {
                            flag = true;
                            dataItem2.IsDefault = true;
                        }
                    }
                }
            }

            if (flag)
            {
                dataItem1.Children.RemoveAt(index1);
                break;
            }
        }

        if (dataItem1.Children.Count != 0)
        {
            return;
        }

        dataItem1.Dispose();
    }

    private DataItem getDifferenceGroup(ItemPath path, string dataItemName)
    {
        if (this["differences"] != null)
        {
            foreach (var dataItem in this["differences"])
            {
                if (dataItem.GetString("diffGroupPath") == path.ToString() && dataItem[dataItemName] != null)
                {
                    return dataItem;
                }
            }
        }

        return null;
    }

    private string createDifferenceGroupName()
    {
        var num = 0;
        var flag = false;
        while (!flag)
        {
            flag = true;
            foreach (var dataItem in this["differences"])
            {
                if (dataItem.Name == "diffGroup" + num)
                {
                    flag = false;
                    ++num;
                    break;
                }
            }
        }

        return "diffGroup" + num;
    }

    private ItemPath getRelativeItemPathFromThisEntity(DataItem dataItem)
    {
        return dataItem.FullPath.GetRelativePath(FullPath);
    }
}