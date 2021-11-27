using SQEX.Luminous.Core.Object;
using System.Collections.Generic;

namespace Black.Save.Multiplay
{
    public partial class SaveChocoboDataStruct
    {
        public partial class ChocoboDataStruct
        {
            new public static ObjectType ObjectType { get; private set; }
            private static PropertyContainer fieldProperties;

            public uint nameId_;
            public int personality_;
            public int rarity_;
            public int color_;
            public int stamina_;
            public int jump_;
            public float speed_;
            public int evolValue_;
            public short trainingCnt_;
            public short specialTrainingNum_;
            public short skillLevel_;
            public short limitMaxLevel_;
            public short maxLevel_;
            public short level_;
            public short injuryPercent_;
            public bool isNew_;
            public bool isInjured_;
            public short trainingBonusCnt_;
            public short trainingRateCnt_;


            new public static void SetupObjectType()
            {
                if (ObjectType != null)
                {
                    return;
                }

                var dummy = new ChocoboDataStruct();
                var properties = dummy.GetFieldProperties();

                ObjectType = new ObjectType("Black.Save.Multiplay.SaveChocoboDataStruct.ChocoboDataStruct", 0, Black.Save.Multiplay.SaveChocoboDataStruct.ChocoboDataStruct.ObjectType, null, properties, 0, 84);
            }

            public ObjectType GetObjectType()
            {
                return ObjectType;
            }

            protected PropertyContainer GetFieldProperties()
            {
                if (fieldProperties != null)
                {
                    return fieldProperties;
                }

                fieldProperties = new PropertyContainer("Black.Save.Multiplay.SaveChocoboDataStruct.ChocoboDataStruct", null, -1194759322, 295722633);
                return fieldProperties;
            }
        }
    }
}