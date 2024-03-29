﻿using UnityEngine;
using Varwin.Public;
using System.Threading;
using RobboVarwin;
using VirtualModule;
using static RobboVarwin.Values;

namespace Varwin.Types.VirtualRobbo_d948cb30690c4f29936b5f7625e2487f
{
    [Locale(SystemLanguage.English, "Virtual ROBBO")]
    [Locale(SystemLanguage.Russian, "Виртуальный РОББО")]
    public class VirtualRobbo : RobboVarwin.Robot
    {
        private SensorColors[] AllColors = {
            SensorColors.Red, SensorColors.Magenta, SensorColors.Yellow,
            SensorColors.Green, SensorColors.Blue, SensorColors.Cyan,
            SensorColors.Black, SensorColors.Gray, SensorColors.White
        };

        private void UpdateWheels()
        {
            UpdateWheelVelocity(LeftWheel, MaximumTorgue * _LeftMotor_Percentage / 100F * _LeftMotor_Direction);
            UpdateWheelVelocity(RightWheel, MaximumTorgue * _RightMotor_Percentage / 100F * _RightMotorDirection);
        }

        private void UpdateWheelVelocity(HingeJoint joint, float velocity)
        {
            var motor = joint.motor;
            motor.targetVelocity = velocity;
            motor.force = 20;
            joint.motor = motor;
        }

        private float _LeftMotor_Percentage = 30;
        public float LeftMotor_Percentage
        {
            get
            {
                return _LeftMotor_Percentage;
            }
            set
            {
                _LeftMotor_Percentage = value;
                UpdateWheelVelocity(LeftWheel, MaximumTorgue * _LeftMotor_Percentage / 100F * _LeftMotor_Direction);
            }
        }

        private float _RightMotor_Percentage = 30;
        public float RightMotor_Percentage
        {
            get
            {
                return _RightMotor_Percentage;
            }
            set
            {
                _RightMotor_Percentage = value;
                UpdateWheelVelocity(RightWheel, MaximumTorgue * _RightMotor_Percentage / 100F * _RightMotorDirection);
            }
        }

        private float _LeftMotor_Direction = 1;
        public float LeftMotor_Direction
        {
            get
            {
                return _LeftMotor_Direction;
            }
            set
            {
                _LeftMotor_Direction = value;
                UpdateWheelVelocity(LeftWheel, MaximumTorgue * _LeftMotor_Percentage / 100F * _LeftMotor_Direction);
            }
        }

        private float _RightMotorDirection = 1;
        private float RightMotor_Direction
        {
            get
            {
                return _RightMotorDirection;
            }
            set
            {
                _RightMotorDirection = value;
                UpdateWheelVelocity(RightWheel, MaximumTorgue * _RightMotor_Percentage / 100F * _RightMotorDirection);
            }
        }

        private float MaximumTorgue = 700F;
        private float ClawTargetAngle = 0F;

        private bool LeftWheel_UseMotor = false;
        private bool RightWheel_UseMotor = false;

        private bool LeftWheel_IsKinematic = false;
        private bool RightWheel_IsKinematic = false;

        private Rigidbody LeftWheel_Rigidbody;
        private Rigidbody RightWheel_Rigidbody;

        public HingeJoint LeftWheel;
        public HingeJoint RightWheel;

        public VirtualWheelEncoder LeftEncoder;
        public VirtualWheelEncoder RightEncoder;

        public VirtualStartButton StartButton;

        public VirtualMagnetPlate Plate1;
        public VirtualMagnetPlate Plate2;
        public VirtualMagnetPlate Plate3;
        public VirtualMagnetPlate Plate4;
        public VirtualMagnetPlate Plate5;

        public HingeJoint LeftClaw;
        public HingeJoint RightClaw;

        void Start()
        {
            LeftMotor_Direction = 1;
            RightMotor_Direction = 1;
            LeftMotor_Percentage = 30;
            RightMotor_Percentage = 30;

            LeftWheel_Rigidbody = LeftWheel.GetComponent<Rigidbody>();
            RightWheel_Rigidbody = RightWheel.GetComponent<Rigidbody>();

            Physics.IgnoreLayerCollision(15, 16, true);
            Physics.IgnoreLayerCollision(15, 17, true);
            Physics.IgnoreLayerCollision(16, 17, true);
        }

        private void Update()
        {
            var motor = LeftClaw.motor;
            var distance = Mathf.DeltaAngle(LeftClaw.gameObject.transform.localEulerAngles.z, -ClawTargetAngle);
            motor.targetVelocity = distance * 0.7F;
            LeftClaw.motor = motor;

            motor = RightClaw.motor;
            distance = Mathf.DeltaAngle(RightClaw.gameObject.transform.localEulerAngles.z, ClawTargetAngle);
            motor.targetVelocity = distance * 0.7F;
            RightClaw.motor = motor;

            LeftWheel.useMotor = LeftWheel_UseMotor;
            RightWheel.useMotor = RightWheel_UseMotor;

            LeftWheel_Rigidbody.isKinematic = LeftWheel_IsKinematic;
            RightWheel_Rigidbody.isKinematic = RightWheel_IsKinematic;
        }

        public override void MotorsOff()
        {
            LeftWheel_UseMotor = false;
            RightWheel_UseMotor = false;

            LeftWheel_IsKinematic = true;
            RightWheel_IsKinematic = true;
            Thread.Sleep(1);
            LeftWheel_IsKinematic = false;
            RightWheel_IsKinematic = false;
        }

        public override void MotorsOn()
        {
            LeftWheel_UseMotor = true;
            RightWheel_UseMotor = true;
        }

        public override void MotorsOnForSeconds(float seconds)
        {
            MotorsOn();
            Thread.Sleep(Mathf.RoundToInt(seconds * 1000F));
            MotorsOff();
        }

        public override void MotorsOnForSteps(int steps)
        {
            var leftSteps = LeftEncoder.Steps;
            var rightSteps = RightEncoder.Steps;

            MotorsOn();

            while (true)
            {
                if ((LeftEncoder.Steps - leftSteps) >= steps)
                {
                    LeftWheel_UseMotor = false;
                }

                if ((RightEncoder.Steps - rightSteps) >= steps)
                {
                    RightWheel_UseMotor = false;
                }

                if ((LeftEncoder.Steps - leftSteps) >= steps && (RightEncoder.Steps - rightSteps) >= steps)
                {
                    break;
                }
            }

            MotorsOff();
        }

        public override void ResetTripMeters()
        {
            LeftEncoder.Reset();
            RightEncoder.Reset();
        }

        private float DirectionToFloat(string direction)
        {
            switch (direction)
            {
                case Values.MotorDirection_Forward:
                    return 1F;
                case Values.MotorDirection_Backward:
                    return -1F;
                default:
                    return 1F;
            }
        }

        public override void SetMotors(string leftDirection, string rightDirection, float leftPercentage, float rightPercentage)
        {
            LeftMotor_Direction = DirectionToFloat(leftDirection);
            RightMotor_Direction = DirectionToFloat(rightDirection);

            LeftMotor_Percentage = leftPercentage;
            RightMotor_Percentage = rightPercentage;
        }

        public override void SetMotorsPower(float percentage)
        {
            LeftMotor_Percentage = RightMotor_Percentage = Mathf.Abs(percentage);
        }

        public override void SetMotorsPower(float left, float right)
        {
            LeftMotor_Percentage = Mathf.Abs(left);
            RightMotor_Percentage = Mathf.Abs(right);
        }

        public override void SetRobotDirection(string direction)
        {
            switch (direction)
            {
                case Values.RobotDirection_Forward:
                    LeftMotor_Direction = RightMotor_Direction = 1F;
                    break;
                case Values.RobotDirection_Backward:
                    LeftMotor_Direction = RightMotor_Direction = -1F;
                    break;
                case Values.RobotDirection_TurnLeft:
                    LeftMotor_Direction = -1F;
                    RightMotor_Direction = 1F;
                    break;
                case Values.RobotDirection_TurnRight:
                    LeftMotor_Direction = 1F;
                    RightMotor_Direction = -1F;
                    break;
            }
        }

        public override void TurnLeftDegrees(float degrees)
        {
            UpdateWheelVelocity(LeftWheel, MaximumTorgue * -0.3F);
            UpdateWheelVelocity(RightWheel, MaximumTorgue * 0.3F);
            MotorsOnForSteps(Mathf.RoundToInt(degrees / 90F * 17F));
            UpdateWheels();
        }

        public override void TurnRightDegrees(float degrees)
        {
            UpdateWheelVelocity(LeftWheel, MaximumTorgue * 0.3F);
            UpdateWheelVelocity(RightWheel, MaximumTorgue * -0.3F);
            MotorsOnForSteps(Mathf.RoundToInt(degrees / 90F * 17F));
            UpdateWheels();
        }

        public override float GetSensorValue(Values.Sensor sensor)
        {
            switch (sensor)
            {
                case Values.Sensor.TripMeterL:
                    return LeftEncoder.Steps;
                case Values.Sensor.TripMeterR:
                    return RightEncoder.Steps;
                case Values.Sensor.Sensor1:
                    return Plate1.Read<float>() ?? -1F;
                case Values.Sensor.Sensor2:
                    return Plate2.Read<float>() ?? -1F;
                case Values.Sensor.Sensor3:
                    return Plate3.Read<float>() ?? -1F;
                case Values.Sensor.Sensor4:
                    return Plate4.Read<float>() ?? -1F;
                case Values.Sensor.Sensor5:
                    return Plate5.Read<float>() ?? -1F;
                default:
                    return -1F;
            }
        }

        public override bool StartButtonPressed()
        {
            return StartButton.Pressed;
        }

        public override void LedOn(string position)
        {
            switch (position)
            {
                case Values.RobotLedPosition_1:
                    Plate1.Write(true);
                    break;
                case Values.RobotLedPosition_2:
                    Plate2.Write(true);
                    break;
                case Values.RobotLedPosition_3:
                    Plate3.Write(true);
                    break;
                case Values.RobotLedPosition_4:
                    Plate4.Write(true);
                    break;
                case Values.RobotLedPosition_5:
                    Plate5.Write(true);
                    break;
                default:
                    break;
            }
        }

        public override void LedOff(string position)
        {
            switch (position)
            {
                case Values.RobotLedPosition_1:
                    Plate1.Write(false);
                    break;
                case Values.RobotLedPosition_2:
                    Plate2.Write(false);
                    break;
                case Values.RobotLedPosition_3:
                    Plate3.Write(false);
                    break;
                case Values.RobotLedPosition_4:
                    Plate4.Write(false);
                    break;
                case Values.RobotLedPosition_5:
                    Plate5.Write(false);
                    break;
                default:
                    break;
            }
        }

        public override Vector3 GetRGBSensorValue(Values.Sensor sensor)
        {
            switch (sensor)
            {
                case Values.Sensor.Sensor1:
                    return Plate1.Read<Vector3>() ?? new Vector3(-1F, -1F, -1F);
                case Values.Sensor.Sensor2:
                    return Plate2.Read<Vector3>() ?? new Vector3(-1F, -1F, -1F);
                case Values.Sensor.Sensor3:
                    return Plate3.Read<Vector3>() ?? new Vector3(-1F, -1F, -1F);
                case Values.Sensor.Sensor4:
                    return Plate4.Read<Vector3>() ?? new Vector3(-1F, -1F, -1F);
                case Values.Sensor.Sensor5:
                    return Plate5.Read<Vector3>() ?? new Vector3(-1F, -1F, -1F);
                default:
                    return new Vector3(-1F, -1F, -1F);
            }
        }

        public override void ClawClosed(float percentage)
        {
            ClawTargetAngle = 80F / 100F * percentage;
        }

        public override void ClawPosition(Values.ClawPosition position)
        {
            switch (position)
            {
                case Values.ClawPosition.Closed:
                    ClawTargetAngle = 0F;
                    break;
                case Values.ClawPosition.HalfOpen:
                    ClawTargetAngle = 40F;
                    break;
                case Values.ClawPosition.Open:
                    ClawTargetAngle = 80F;
                    break;
            }
        }

        private Vector3 MapColor(Values.SensorColors color)
        {
            switch (color)
            {
                case Values.SensorColors.Black:
                    return (Vector4)Color.black;
                case Values.SensorColors.Blue:
                    return (Vector4)Color.blue;
                case Values.SensorColors.Cyan:
                    return (Vector4)Color.cyan;
                case Values.SensorColors.Gray:
                    return (Vector4)Color.gray;
                case Values.SensorColors.Green:
                    return (Vector4)Color.green;
                case Values.SensorColors.Magenta:
                    return (Vector4)Color.magenta;
                case Values.SensorColors.Red:
                    return (Vector4)Color.red;
                case Values.SensorColors.White:
                    return (Vector4)Color.white;
                case Values.SensorColors.Yellow:
                    return (Vector4)Color.yellow;
                default:
                    return new Vector3(-1F, -1F, -1F);
            }
        }

        public override bool ColorSensor(Values.Sensor sensor, Values.SensorColors color)
        {
            var sensor_color = GetRGBSensorValue(sensor) / 255F;

            switch (color)
            {
                case Values.SensorColors.Unknown:
                    bool isUnknown = false;

                    foreach (var test_color in AllColors)
                    {
                        isUnknown |= (sensor_color - MapColor(test_color)).magnitude < 0.25F;
                    }

                    return isUnknown;
                default:
                    var distance = (sensor_color - MapColor(color)).magnitude;
                    return distance < 0.25F;
            }
        }
    }
}
