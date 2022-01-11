﻿using ControllerCommon;
using ControllerService.Targets;
using Microsoft.Extensions.Logging;
using SharpDX.DirectInput;
using SharpDX.XInput;
using System.Collections.Generic;

namespace ControllerService
{
    public class XInputController
    {
        public Controller physicalController;
        public ViGEmTarget virtualTarget;

        public Profile profile;
        private Profile defaultProfile;

        public DeviceInstance Instance;

        public XInputGirometer Gyrometer;
        public XInputAccelerometer Accelerometer;
        public XInputInclinometer Inclinometer;

        public UserIndex UserIndex;
        private readonly ILogger logger;

        public XInputController(Controller controller, UserIndex index, ILogger logger)
        {
            this.logger = logger;

            // initilize controller
            this.physicalController = controller;
            this.UserIndex = index;

            // initialize profile(s)
            profile = new();
            defaultProfile = new();
        }

        public Dictionary<string, string> ToArgs()
        {
            return new Dictionary<string, string>() {
                { "ProductName", Instance.ProductName },
                { "InstanceGuid", $"{Instance.InstanceGuid}" },
                { "ProductGuid", $"{Instance.ProductGuid}" },
                { "ProductIndex", $"{(int)UserIndex}" }
            };
        }

        public void SetProfile(Profile profile)
        {
            // skip if current profile
            if (profile == this.profile)
                return;

            // restore default profile
            if (profile == null)
                profile = defaultProfile;

            this.profile = profile;

            // update default profile
            if (profile.IsDefault)
                defaultProfile = profile;
            else
                logger.LogInformation("Profile {0} applied.", profile.name);
        }

        public void SetGyroscope(XInputGirometer gyrometer)
        {
            Gyrometer = gyrometer;
        }

        public void SetAccelerometer(XInputAccelerometer accelerometer)
        {
            Accelerometer = accelerometer;
        }

        public void SetInclinometer(XInputInclinometer inclinometer)
        {
            Inclinometer = inclinometer;
        }

        public void SetTarget(ViGEmTarget target)
        {
            this.virtualTarget = target;

            Gyrometer.ReadingChanged += virtualTarget.Girometer_ReadingChanged;
            Accelerometer.ReadingHasChanged += virtualTarget.Accelerometer_ReadingChanged;

            logger.LogInformation("Virtual {0} attached to {1} on slot {2}", target, Instance.InstanceName, UserIndex);
            logger.LogInformation("Virtual {0} report interval set to {1}ms", target, virtualTarget.UpdateTimer.Interval);
        }
    }
}
