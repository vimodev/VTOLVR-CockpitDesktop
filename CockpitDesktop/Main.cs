using Harmony;
using System.Reflection;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;

namespace CockpitDesktop
{
    public class Main : VTOLMOD
    {

        // Windows kernel functions for loading uDesktopDuplication dll (unmanaged)
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

        // Harmony ID for harmony patches
        private const string __harmonyID = "vimodev.cockpitdesktop";

        // In game plane for rendering desktop onto MFD
        public static GameObject mfdDesktop;
        // uDesktopDuplication texture containing desktop
        public static uDesktopDuplication.Texture texture;
        // Was the camera active previous frame?
        public static bool cameraActivePrev = false;

        // Called when mod is first loaded
        public override void ModLoaded()
        {
            // Load uDesktopDuplication dll and assets
            LoadDll();
            AssetBundle bundle = LoadAssetBundle();
            // Patch the game
            HarmonyInstance instance = HarmonyInstance.Create(__harmonyID);
            instance.PatchAll(Assembly.GetExecutingAssembly());
            VTOLAPI.SceneLoaded += SceneLoaded;
            base.ModLoaded();
        }

        // Every frame
        void Update()
        {
            
            // Update the MFD desktop displayer
            UpdateMFD();
        }

        // Update the MFD display of the desktop
        void UpdateMFD()
        {
            
            if (ExternalCamManager.instance == null) return;
            List<Camera> cameras = ExternalCamManager.instance.cameras;
            if (cameras.Count == 0) return;
            if (cameras[0].gameObject.activeSelf)
            {
                if (!cameraActivePrev) CreateMFDDesktop();
                cameraActivePrev = true;
                cameras[0].targetTexture.Release();
                cameras[0].targetTexture.enableRandomWrite = true;
                Graphics.Blit(texture.material.mainTexture, cameras[0].targetTexture, new Vector2(1, -1), Vector2.zero);
                cameras[0].targetTexture.Create();
            } else if (cameraActivePrev)
            {
                DestroyMFDDesktop();
                cameraActivePrev = false;
            }
        }

        private void SceneLoaded(VTOLScenes scene)
        {
        }

        public void DestroyMFDDesktop()
        {
            if (mfdDesktop != null) Destroy(mfdDesktop);
            mfdDesktop = null; texture = null;
        }

        public void CreateMFDDesktop()
        {
            DestroyMFDDesktop();
            mfdDesktop = GameObject.CreatePrimitive(PrimitiveType.Plane);
            mfdDesktop.AddComponent<uDesktopDuplication.Texture>();
            texture = mfdDesktop.GetComponent<uDesktopDuplication.Texture>();
            if (texture == null) Log("Texture is null??");
            // Set desktop to invisible location
            mfdDesktop.transform.localScale = Vector3.one * 0.1f;
            mfdDesktop.transform.position = new Vector3(0, -1000, 0);
        }

        public void LoadDll()
        {
            string folder = this.ModFolder;
            folder = Path.Combine(folder, "uDesktopDuplication");
            if (Environment.Is64BitProcess)
            {
                folder = Path.Combine(folder, "x64");
            } else
            {
                folder = Path.Combine(folder, "x86");
            }
            Log("Loading dll from folder: " + folder);
            LoadLibrary(Path.Combine(folder, "uDesktopDuplication.dll"));
            if (GetModuleHandle("uDesktopDuplication.dll") == null)
            {
                Log("Failed to load dll.");
            }
        }

        public AssetBundle LoadAssetBundle()
        {
            string folder = this.ModFolder;
            folder = Path.Combine(folder, "uDesktopDuplication");
            Log("Loading asset bundle from folder: " + folder);
            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(folder, "desktop"));
            if (assetBundle == null) Log("Failed to load asset bundle.");
            return assetBundle;
        }
    }
}