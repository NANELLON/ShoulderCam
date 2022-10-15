using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Security.AccessControl;
using System.Windows.Forms;

namespace AimCam
{
    public class Main : Script
    {
        int view;
        float deathHealth;
        bool cameraSet = false;
        Camera headCam;
        int replaceCam;
        bool mode;
        int delay;
        float sidetoside = 0.4f;
        float backtofront = -0.5f;
        float rotation = 90.0f;
        Keys switchKey;
        public Main()
        {
           deathHealth = Settings.GetValue("SETTINGS", "deathHealth", 0.0f);
           switchKey = Settings.GetValue("SETTINGS", "Key", Keys.X);
           replaceCam = Settings.GetValue("SETTINGS", "replaceCam", 2);
           mode = Settings.GetValue("SETTINGS", "aimOnly", false);
           delay = Settings.GetValue("SETTINGS", "resetDelay", 3000);
           Tick += onTick;
           KeyUp += onKeyUp;

        }
        void onKeyUp(object sender, KeyEventArgs e)
        {
            Ped player = Game.Player.Character;
            if (cameraSet && !player.IsInVehicle()) 
            {
                if (e.KeyCode == switchKey && sidetoside >= 0.4f)
                {
                    while (sidetoside > -0.5f)
                    {
                        Function.Call(Hash._ATTACH_CAM_TO_PED_BONE_2, headCam, player, 31086, 0.0f, rotation, 0.0f, sidetoside, backtofront, 0.07f, true);
                        //GTA.UI.Screen.ShowSubtitle("side:" + sidetoside + "back:" + backtofront, 2000);
                        sidetoside = sidetoside - 0.1f;
                        backtofront = backtofront + 0.016f;
                        rotation = rotation - 0.5f;
                        if (sidetoside < -0.5f)
                        {
                            sidetoside = -0.5f;
                        }
                        Wait(1);
                    }
                }
                else if (e.KeyCode == switchKey && sidetoside <= -0.5f)
                {
                    while (sidetoside < 0.4f)
                    {
                        Function.Call(Hash._ATTACH_CAM_TO_PED_BONE_2, headCam, player, 31086, 0.0f, rotation, 0.0f, sidetoside, backtofront, 0.07f, true);
                        //GTA.UI.Screen.ShowSubtitle("side:" + sidetoside + "back:" + backtofront, 2000);
                        sidetoside = sidetoside + 0.1f;
                        backtofront = backtofront - 0.016f;
                        rotation = rotation + 0.5f;
                        if (sidetoside > 0.4f)
                        {
                            sidetoside = 0.4f;
                        }
                        Wait(1);
                    }
                }
            }
           
        }
        void resetCamera()
        {
            if (cameraSet)
            {
                World.RenderingCamera = null;
                World.DestroyAllCameras();
                cameraSet = false;
                //  Function.Call(Hash.SET_TIMECYCLE_MODIFIER_STRENGTH, 0.0f);
            }
        }
        void setCamera()
        {
            Ped player = Game.Player.Character;
            if (!cameraSet) 
            {
                headCam = World.CreateCamera(player.Position, player.Rotation, 100.0f);
                Function.Call(Hash._ATTACH_CAM_TO_PED_BONE_2, headCam, player, 31086, 0.0f, rotation, 0.0f, sidetoside, backtofront, 0.07f, true);
                World.RenderingCamera = headCam;
                cameraSet = true;
            }
            
            if (cameraSet)
            {
               // Function.Call(Hash.SET_TIMECYCLE_MODIFIER, "secret_camera");
               // Function.Call(Hash.SET_TIMECYCLE_MODIFIER_STRENGTH, 1.0f);
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
            
//TEMP
            if (player.Health <= deathHealth)
            {
                resetCamera();
            }
//\TEMP
            if (!player.IsInVehicle())
            {
                if(cameraSet && World.RenderingCamera != headCam)
                {
                    Wait(delay);
                    resetCamera();
                    Wait(1);
                    setCamera();
                }
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
                    if (view == replaceCam && player.Health > deathHealth)
                    {
                        setCamera();
                    }
                    if (view != replaceCam)
                    {
                        resetCamera();
                    }
                }
            }
        }
    }
}
