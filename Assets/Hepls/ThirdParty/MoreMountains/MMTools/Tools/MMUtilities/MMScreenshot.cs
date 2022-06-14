using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Utilities/MMScreenshot")]
    public class MMScreenshot : MonoBehaviour
    {
        public string FolderName = "Screenshots";
        public enum Methods { ScreenCapture, RenderTexture }

        [Header("Screenshot")]
        public Methods Method = Methods.ScreenCapture;
        public KeyCode ScreenshotShortcut = KeyCode.K;
        [MMEnumCondition("Method", (int)Methods.ScreenCapture)]        
        public int GameViewSizeMultiplier = 3;
        [MMEnumCondition("Method", (int)Methods.RenderTexture)]        
        public Camera TargetCamera;
        [MMEnumCondition("Method", (int)Methods.RenderTexture)]
        public int ResolutionWidth;
        [MMEnumCondition("Method", (int)Methods.RenderTexture)]
        public int ResolutionHeight;

        [Header("Controls")]
        [MMInspectorButton("TakeScreenshot")]
        public bool TakeScreenshotButton;
        protected virtual void LateUpdate()
        {
            DetectInput();
        }
        protected virtual void DetectInput()
        {
            if (Input.GetKeyDown(ScreenshotShortcut))
            {
                TakeScreenshot();
            }
        }
        protected virtual void TakeScreenshot()
        {
            if (!Directory.Exists(FolderName))
            {
                Directory.CreateDirectory(FolderName);
            }

            string savePath = "";
            switch (Method)
            {
                case Methods.ScreenCapture:
                    savePath = TakeScreenCaptureScreenshot();
                    break;

                case Methods.RenderTexture:
                    savePath = TakeRenderTextureScreenshot();
                    break;
            }
            Debug.Log("[MMScreenshot] Screenshot taken and saved at " + savePath);
        }
        protected virtual string TakeScreenCaptureScreenshot()
        {
            float width = Screen.width * GameViewSizeMultiplier;
            float height = Screen.height * GameViewSizeMultiplier;
            string savePath = FolderName+"/screenshot_" + width + "x" + height + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

            ScreenCapture.CaptureScreenshot(savePath, GameViewSizeMultiplier);
            return savePath;
        }
        protected virtual string TakeRenderTextureScreenshot()
        {
            string savePath = FolderName + "/screenshot_" + ResolutionWidth + "x" + ResolutionHeight + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

            RenderTexture renderTexture = new RenderTexture(ResolutionWidth, ResolutionHeight, 24);
            TargetCamera.targetTexture = renderTexture;
            Texture2D screenShot = new Texture2D(ResolutionWidth, ResolutionHeight, TextureFormat.RGB24, false);
            TargetCamera.Render();
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(new Rect(0, 0, ResolutionWidth, ResolutionHeight), 0, 0);
            TargetCamera.targetTexture = null;
            RenderTexture.active = null; 
            Destroy(renderTexture);
            byte[] bytes = screenShot.EncodeToPNG();
            System.IO.File.WriteAllBytes(savePath, bytes);

            return savePath;
        }
    }
}
