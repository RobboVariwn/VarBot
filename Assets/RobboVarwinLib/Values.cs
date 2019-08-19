using System.Collections;
using System.Collections.Generic;
using UnityEngine;namespace RobboVarwin
{
    public static class Values
    {        public const string RobotDirection_Forward = "RobotDirection_Forward";        public const string RobotDirection_Backward = "RobotDirection_Backward";        public const string RobotDirection_TurnLeft = "RobotDirection_TurnLeft";        public const string RobotDirection_TurnRight = "RobotDirection_TurnRight";        public const string MotorDirection_Forward = "MotorDirection_Forward";        public const string MotorDirection_Backward = "MotorDirection_Backward";

        public const string RobotLedPosition_1 = "RobotLedPosition_1";
        public const string RobotLedPosition_2 = "RobotLedPosition_2";
        public const string RobotLedPosition_3 = "RobotLedPosition_3";
        public const string RobotLedPosition_4 = "RobotLedPosition_4";
        public const string RobotLedPosition_5 = "RobotLedPosition_5";

        public enum Sensor
        {
            Sensor1,
            Sensor2,
            Sensor3,
            Sensor4,
            Sensor5,
            TripMeterL,
            TripMeterR,
        }

        public enum ClawPosition
        {
            Open,
            HalfOpen,
            Closed,
        }
    }
}