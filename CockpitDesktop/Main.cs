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

        public static Main instance;

        // Harmony ID for harmony patches
        private const string __harmonyID = "vimodev.cockpitdesktop";

        // In game plane for rendering desktop onto MFD
        public static GameObject mfdDesktop;
        // uDesktopDuplication texture containing desktop
        public static uDesktopDuplication.Texture texture;

        // Called when mod is first loaded
        public override void ModLoaded()
        {
            Main.instance = this;
            LoadDll();
            // Patch the game if necessary
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
            // Edge cases, skip
            if (ExternalCamManager.instance == null) return;
            List<Camera> cameras = ExternalCamManager.instance.cameras;
            if (cameras.Count == 0) return;
            // Go over all cameras
            RenderTexture target = ExternalCamManager.instance.renderTexture;
            for (int i = 0; i < cameras.Count; i++)
            {
                // As long as there are monitors
                if (i >= uDesktopDuplication.Manager.monitorCount) break;
                // If the camera is active, we render the screen over the camera
                if (cameras[i].gameObject.activeSelf)
                {
                    texture.monitorId = i;
                    // Write over the camera's target texture
                    target.Release();
                    target.width = texture.material.mainTexture.width;
                    target.height = texture.material.mainTexture.height;
                    target.enableRandomWrite = true;
                    Graphics.Blit(texture.material.mainTexture, target, new Vector2(1, -1), Vector2.zero);
                    // Finalize the texture
                    target.Create();
                    break;
                }
            }
        }

        // On scene load, destroy and recreate the screen
        private void SceneLoaded(VTOLScenes scene)
        {
            DestroyMFDDesktop();
            CreateMFDDesktop();
        }

        // Destroy the screen
        public void DestroyMFDDesktop()
        {
            if (mfdDesktop != null) Destroy(mfdDesktop);
            mfdDesktop = null; texture = null;
        }

        // Create the screen
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

        // Load the uDesktopDuplication dll
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

    }
}