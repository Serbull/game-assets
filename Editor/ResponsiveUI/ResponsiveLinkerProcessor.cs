using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEditor.UnityLinker;

namespace Serbull.ResponsiveUI.EditorTools
{
    // Package link.xml files must be supplied explicitly to UnityLinker.
    public sealed class ResponsiveLinkerProcessor : IUnityLinkerProcessor
    {
        public int callbackOrder => 0;

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            var package = PackageInfo.FindForAssembly(typeof(RectTransformVariant).Assembly);
            if (package == null)
                throw new BuildFailedException("Responsive UI must be installed through Game Assets.");

            var path = Path.Combine(package.resolvedPath, "Runtime", "ResponsiveUI", "link.xml");
            if (!File.Exists(path))
                throw new BuildFailedException("Responsive UI linker configuration is missing: " + path);

            return path;
        }
    }
}
