using System.IO;
using Flagrum.Core.Animation;

namespace Flagrum.Console.Scripts.Animation;

public static class AnimationPackScripts
{
    /// <summary>
    /// Replaces animations in a PKA file with the given replacement animation files
    /// </summary>
    /// <param name="pkaFilePath">The path to the PKA that you want to edit</param>
    /// <param name="outputFilePath">The path where you want to save the modified PKA</param>
    /// <param name="replacements">A list of index + replacement path pairs corresponding to the animations you wish to replace</param>
    public static void ReplaceAnimations(string pkaFilePath, string outputFilePath,
        params (int index, string newAniPath)[] replacements)
    {
        // Load the PKA into memory
        var data = File.ReadAllBytes(pkaFilePath);
        var pack = AnimationPackage.FromData(data);

        // Swap out the animation(s)
        foreach (var (index, newAniPath) in replacements)
        {
            pack.ReplaceAnimation(index, newAniPath);
        }

        // Repack the PKA
        var outputData = AnimationPackage.ToData(pack);
        File.WriteAllBytes(outputFilePath, outputData);
    }
}