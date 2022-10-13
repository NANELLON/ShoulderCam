using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Security.AccessControl;

namespace AimCam
{
    public class Main : Script
    {
        int view;
        bool cameraSet = false;
        Camera headCam;
        int replaceCam;
        bool mode;
        public Main()
        {
           replaceCam = Settings.GetValue("SETTINGS", "replaceCam", 2);
           mode = Settings.GetValue("SETTINGS", "aimOnly", false);
            Tick += onTick;

        }
        void resetCamera()
        {
            World.RenderingCamera = null;
            World.DestroyAllCameras();
            cameraSet = false;
            Function.Call(Hash.SET_TIMECYCLE_MODIFIER_STRENGTH, 0.0f);
        }
        void setCamera()
        {
            Ped player = Game.Player.Character;
            if (!cameraSet) 
            {
                headCam = World.CreateCamera(player.Position, player.Rotation, 100.0f);
                Function.Call(Hash._ATTACH_CAM_TO_PED_BONE_2, headCam, player, 31086, 0.0f, 90.0f, -0.5f, 0.4f, -0.5f, 0.07f, true);
                World.RenderingCamera = headCam;
                cameraSet = true;
            }
            
            if (cameraSet)
            {
                Function.Call(Hash.SET_TIMECYCLE_MODIFIER, "secret_camera");
                Function.Call(Hash.SET_TIMECYCLE_MODIFIER_STRENGTH, 0.0f);
                GameplayCamera.ClampPitch(0.0f, 0.0f);
                GameplayCamera.ClampYaw(0.0f, 0.0f);

                if (player.IsAiming || player.IsReloading)
                {
                    GameplayCamera.ClampPitch(-360.0f, 360.0f);
                    GameplayCamera.ClampYaw(-360.0f, 360.0f);
                    Function.Call(Hash.SET_CAM_AFFECTS_AIMING, headCam, false);
                }
            }
        }
        void onTick(object sender, EventArgs e)
        {
            Ped player = Game.Player.Character;
            if (mode)
            {
                view = Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE);
                if (view != 4)
                {
                    if (player.IsAiming || !player.IsAiming && player.IsReloading)
                    {
                        setCamera();
                    }
                    if (!player.IsAiming && !player.IsReloading)
                    {
                        resetCamera();
                    }
                }
              
            }
            if (!mode)
            {
                view = Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE);
                if (view == replaceCam)
                {
                    setCamera();
                }
                if(view != replaceCam)
                {
                    resetCamera();
                }
            }
            
           
            
        }
    }
}
