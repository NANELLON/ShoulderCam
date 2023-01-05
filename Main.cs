using System;
using GTA;
using GTA.Native;
using GTA.Math;
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
        float sidetoside = 0.19f;
        float backtofront = -0.07f;
        float rotation = 85f;
        public Main()
        {
           deathHealth = Settings.GetValue("SETTINGS", "deathHealth", 0.0f);
           replaceCam = Settings.GetValue("SETTINGS", "replaceCam", 2);
           mode = Settings.GetValue("SETTINGS", "aimOnly", false);
           delay = Settings.GetValue("SETTINGS", "resetDelay", 3000);
           Tick += onTick;

        }
        Vector3 rayCast()
        {
            //get Aim Postion
            Vector3 camPos = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_COORD);
            Vector3 camRot = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_ROT);
            float retz = camRot.Z * 0.0174532924F;
            float retx = camRot.X * 0.0174532924F;
            float absx = (float)Math.Abs(Math.Cos(retx));
            Vector3 camStuff = new Vector3((float)Math.Sin(retz) * absx * -1, (float)Math.Cos(retz) * absx, (float)Math.Sin(retx));
            //AimPostion Result
            RaycastResult ray = World.Raycast(camPos, camPos + camStuff * 1000, IntersectFlags.Everything);
            Vector3 trg = ray.HitPosition;
            return trg;
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

            if (cameraSet)
            {
                Function.Call(Hash.DISPLAY_RADAR, false);
                // Function.Call(Hash.SET_TIMECYCLE_MODIFIER, "secret_camera");
                // Function.Call(Hash.SET_TIMECYCLE_MODIFIER_STRENGTH, 1.0f);
                GameplayCamera.ClampPitch(0.0f, 0.0f);
                GameplayCamera.ClampYaw(0.0f, 0.0f);
                if (player.IsAiming || player.IsReloading)
                {
                    Vector3 target = rayCast();
                    float siz = target.DistanceTo(player.Position);
                    siz = siz / 290;
                    Function.Call(Hash._DRAW_SPHERE, target.X, target.Y, target.Z, siz, 255, 0, 0, 1.0f);
                    GameplayCamera.ClampPitch(-360.0f, 360.0f);
                    GameplayCamera.ClampYaw(-360.0f, 360.0f);
                    Function.Call(Hash.SET_CAM_AFFECTS_AIMING, headCam, false);
                }
            }
            else
            if (!cameraSet) 
            {
                headCam = World.CreateCamera(player.Position, player.Rotation, 85.0f);
                Function.Call(Hash._ATTACH_CAM_TO_PED_BONE_2, headCam, player, 31086, 0f, rotation, -8f, sidetoside, backtofront, 0.09f, true);
                
                //Function.Call(Hash._FREEZE_PED_CAMERA_ROTATION, player);
                headCam.NearClip = 0f;
                World.RenderingCamera = headCam;
                cameraSet = true;
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
                if(cameraSet && World.RenderingCamera != headCam)
                {
                    Wait(delay);
                    resetCamera();
                    Wait(1);
                    setCamera();
                }
                
                if (player.IsInVehicle())
                {
                    view = Function.Call<int>(Hash.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE);
                if (player.IsInHeli)
                {
                if(view == 4)
                    {
                        view = replaceCam;
                    }
                }
            }
                else if (!player.IsInVehicle())
                {
                    view = Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE);
                }
                if (!mode)
                {
                    if (view == replaceCam && player.Health > deathHealth)
                    {
                        setCamera();
                    }
                    if (view != replaceCam)
                    {
                        resetCamera();
                    }
                } else if (mode)
                {
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
        }
    }
}
