using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Feedbacks
{
    public class DemoPackageTester : MonoBehaviour
    {
        [MMFInformation("This component is only used to display an error in the console in case dependencies for this demo haven't been installed. You can safely remove it if you want, and typically you wouldn't want to keep that in your own game.", MMFInformationAttribute.InformationType.Warning, false)]
        public bool RequiresPostProcessing;
        public bool RequiresTMP;
        public bool RequiresCinemachine;
        protected virtual void Awake()
        {
            if (Application.isPlaying)
            {
                TestForDependencies();    
            }
        }
        protected virtual void TestForDependencies()
        {
            bool missingDependencies = false;
            string missingString = "";
            bool cinemachineFound = false;
            bool tmpFound = false;
            bool postProcessingFound = false;
            
            #if MOREMOUNTAINS_CINEMACHINE_INSTALLED
                cinemachineFound = true;
            #endif
                        
            #if MOREMOUNTAINS_TEXTMESHPRO_INSTALLED
                tmpFound = true;
            #endif
                        
            #if MOREMOUNTAINS_POSTPROCESSING_INSTALLED
                postProcessingFound = true;
            #endif

            if (missingDependencies)
            {
            }

            if (RequiresCinemachine && !cinemachineFound)
            {
                missingDependencies = true;
                missingString += "Cinemachine";
            }

            if (RequiresTMP && !tmpFound)
            {
                missingDependencies = true;
                if (missingString != "") { missingString += ", "; }
                missingString += "TextMeshPro";
            }
            
            if (RequiresPostProcessing && !postProcessingFound)
            {
                missingDependencies = true;
                if (missingString != "") { missingString += ", "; }
                missingString += "PostProcessing";
            }
            
            #if UNITY_EDITOR
            if (missingDependencies)
            {
                Debug.LogError("[DemoPackageTester] It looks like you're missing some dependencies required by this demo ("+missingString+")." +
                               " You'll have to install them to run this demo. You can learn more about how to do so in the documentation, at http://feel-docs.moremountains.com/how-to-install.html" +
                               "\n\n");
                
                if (EditorUtility.DisplayDialog("Missing dependencies!",
                        "This demo requires a few dependencies to be installed first (Cinemachine, TextMesh Pro, PostProcessing).\n\n" +
                        "You can use Feel without them of course, but this demo needs them to work (check out the documentation to learn more!).\n\n" +
                        "Would you like to automatically install them?", "Yes, install dependencies", "No"))
                {
                    MMFDependencyInstaller.InstallFromPlay();
                }
            }
            #endif
        }
    }    
}

