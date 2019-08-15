﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Varwin;
using Varwin.Public;
using System.Threading;

namespace RobboVarwin
{
    public abstract class Robot : VarwinObject
    {
        public delegate void ThreadStarted();

        [Event("thread_started")]
        [Locale(SystemLanguage.English, "ROBBO thread started")]
        [Locale(SystemLanguage.Russian, "РОББО поток запущен")]
        public event ThreadStarted MainThreadStarted;

        [Value("robot_direction")]
        [HideInInspector]
        [Locale(SystemLanguage.English, "Forward")]
        [Locale(SystemLanguage.Russian, "Вперед")]
        public string RobotDirection_Forward;

        [Value("robot_direction")]
        [HideInInspector]
        [Locale(SystemLanguage.English, "Backward")]
        [Locale(SystemLanguage.Russian, "Назад")]
        public string RobotDirection_Backward;

        [Value("robot_direction")]
        [HideInInspector]
        [Locale(SystemLanguage.English, "Turn left")]
        [Locale(SystemLanguage.Russian, "Налево")]
        public string RobotDirection_TurnLeft;

        [Value("robot_direction")]
        [HideInInspector]
        [Locale(SystemLanguage.English, "Turn right")]
        [Locale(SystemLanguage.Russian, "Направо")]
        public string RobotDirection_TurnRight;



        [Value("motor_direction")]
        [HideInInspector]
        [Locale(SystemLanguage.English, "Forward")]
        [Locale(SystemLanguage.Russian, "Вперед")]
        public string MotorDirection_Forward;

        [Value("motor_direction")]
        [HideInInspector]
        [Locale(SystemLanguage.English, "Backward")]
        [Locale(SystemLanguage.Russian, "Назад")]
        public string MotorDirection_Backward;



        [Action("start_thread")]
        [Locale(SystemLanguage.English, "Start ROBBO thread")]
        [Locale(SystemLanguage.Russian, "Запустить РОББО поток")]
        public void StartMainThread()
        {
            new Thread(() =>
            {
                MainThreadStarted?.Invoke();
            }).Start();
        }

        [Action("wait_seconds")]
        [Locale(SystemLanguage.English, "Wait for", "seconds")]
        [Locale(SystemLanguage.Russian, "Ждать", "секунд")]
        public void WaitSeconds(float seconds)
        {
            Thread.Sleep(Mathf.RoundToInt(seconds * 1000));
        }

        [Action("motors_on_for_seconds")]
        [Locale(SystemLanguage.English, "Motors on for", "seconds")]
        [Locale(SystemLanguage.Russian, "Включить моторы на", "секунд")]
        public abstract void MotorsOnForSeconds(float seconds);

        [Action("motors_on")]
        [Locale(SystemLanguage.English, "Motors on")]
        [Locale(SystemLanguage.Russian, "Включить моторы")]
        public abstract void MotorsOn();

        [Action("motors_off")]
        [Locale(SystemLanguage.English, "Motors off")]
        [Locale(SystemLanguage.Russian, "Выключить моторы")]
        public abstract void MotorsOff();

        [Action("set_robot_direction")]
        [Values("robot_direction")]
        [Locale(SystemLanguage.English, "Set robot direction to")]
        [Locale(SystemLanguage.Russian, "Уст. направление робота")]
        public abstract void SetRobotDirection(string direction);

        [Action("motors_on_for_steps")]
        [Locale(SystemLanguage.English, "Motors on for", "steps")]
        [Locale(SystemLanguage.Russian, "Включить моторы на", "шагов")]
        public abstract void MotorsOnForSteps(int steps);

        [Action("reset_trip_meters")]
        [Locale(SystemLanguage.English, "Reset trip meters")]
        [Locale(SystemLanguage.Russian, "Сбросить пройденный путь")]
        public abstract void ResetTripMeters();

        [Action("turn_right_degrees")]
        [Locale(SystemLanguage.English, "Turn right", "degrees")]
        [Locale(SystemLanguage.Russian, "Повернуть направо на", "градусов")]
        public abstract void TurnRightDegrees(float degrees);

        [Action("turn_left_degrees")]
        [Locale(SystemLanguage.English, "Turn left", "degrees")]
        [Locale(SystemLanguage.Russian, "Повернуть налево на", "граудсов")]
        public abstract void TurnLeftDegrees(float degrees);

        [Action("set_motors_power")]
        [Locale(SystemLanguage.English, "Set motors power", "%")]
        [Locale(SystemLanguage.Russian, "Уст. мощноcть моторов", "%")]
        public abstract void SetMotorsPower(float percentage);

        [Action("set_lr_motors_power")]
        [Locale(SystemLanguage.English, "Set motors power", "L", "R", "%")]
        [Locale(SystemLanguage.Russian, "Уст. мощноcть моторов", "Л", "П", "%")]
        public abstract void SetMotorsPower(float left, float right);

        [Action("set_lr_motors")]
        [Values("motor_direction")]
        [Locale(SystemLanguage.English, "set direction", "L", "R", "power L", "R", "%")]
        [Locale(SystemLanguage.Russian, "уст. направление", "Л", "П", "мощность Л", "П", "%")]
        public abstract void SetMotors(string leftDirection, string rightDirection, float leftPercentage, float rightPercentage);

        public abstract float GetSensorValue(Values.Sensor sensor);

        [Getter("sensor_1")]
        [Locale(SystemLanguage.English, "Sensor 1")]
        [Locale(SystemLanguage.Russian, "Датчик 1")]
        public float Sensor1
        {
            get
            {
                return GetSensorValue(Values.Sensor.Sensor1);
            }
        }

        [Getter("sensor_2")]
        [Locale(SystemLanguage.English, "Sensor 2")]
        [Locale(SystemLanguage.Russian, "Датчик 2")]
        public float Sensor2
        {
            get
            {
                return GetSensorValue(Values.Sensor.Sensor2);
            }
        }

        [Getter("sensor_3")]
        [Locale(SystemLanguage.English, "Sensor 3")]
        [Locale(SystemLanguage.Russian, "Датчик 3")]
        public float Sensor3
        {
            get
            {
                return GetSensorValue(Values.Sensor.Sensor3);
            }
        }

        [Getter("sensor_4")]
        [Locale(SystemLanguage.English, "Sensor 4")]
        [Locale(SystemLanguage.Russian, "Датчик 4")]
        public float Sensor4
        {
            get
            {
                return GetSensorValue(Values.Sensor.Sensor4);
            }
        }

        [Getter("sensor_5")]
        [Locale(SystemLanguage.English, "Sensor 5")]
        [Locale(SystemLanguage.Russian, "Датчик 5")]
        public float Sensor5
        {
            get
            {
                return GetSensorValue(Values.Sensor.Sensor5);
            }
        }

        [Getter("trip_meter_l")]
        [Locale(SystemLanguage.English, "Trip meter L")]
        [Locale(SystemLanguage.Russian, "Счетчик пути Л")]
        public int TripMeterL
        {
            get
            {
                return Mathf.RoundToInt(GetSensorValue(Values.Sensor.TripMeterL));
            }
        }

        [Getter("trip_meter_r")]
        [Locale(SystemLanguage.English, "Trip meter R")]
        [Locale(SystemLanguage.Russian, "Счетчик пути П")]
        public int TripMeterR
        {
            get
            {
                return Mathf.RoundToInt(GetSensorValue(Values.Sensor.TripMeterR));
            }
        }
    }
}

