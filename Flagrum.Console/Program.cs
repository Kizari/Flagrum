using System;
using System.IO;
using System.Linq;
using Flagrum.Core.Animation.InverseKinematics;
using ProtoBuf;

using var file = File.OpenRead(@"C:\Users\Kieran\Desktop\Models2\nh02\nh02.lik");
var rig = Serializer.Deserialize<IKRig>(file);

Recursion(rig.Joints.Single(j => j.Parent == -1), 0);

void Recursion(IKJoint joint, int indent)
{
    for (var i = 0; i < indent; i++)
    {
        Console.Write("—");
    }
    
    Console.Write(joint.ModelBoneName + "_IK\n");

    if (joint.NonIKChildren != null)
    {
        foreach (var child in joint.NonIKChildren)
        {
            for (var i = 0; i < indent + 1; i++)
            {
                Console.Write("—");
            }

            Console.Write(child + "\n");
        }
    }

    var index = rig.Joints.IndexOf(joint);
    foreach (var child in rig.Joints.Where(j => j.Parent == index))
    {
        Recursion(child, indent + 1);
    }
}

//using var file2 = File.OpenWrite(@"C:\Users\Kieran\Desktop\Models2\nh00\nh00_2.lik");
//Serializer.Serialize(file2, rig);