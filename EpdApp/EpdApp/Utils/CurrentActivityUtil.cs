﻿using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Runtime;
using Android.Util;
using Java.Lang;
using Java.Util;

namespace EpdApp.Utils
{
    public static class CurrentActivityUtil
    {
        /// <summary>
        /// Gets the current activity of the application (must run on the UI thread for older Android versions).
        /// </summary>
        /// <returns>The current activity.</returns>
        /// <param name="app">App.</param>
        public static Activity GetCurrentActivity()
        {
            Activity activity = null;
            List<Object> objects = null;

            var activityThreadClass = Class.ForName("android.app.ActivityThread");
            var activityThread = activityThreadClass.GetMethod("currentActivityThread").Invoke(null);
            var activityFields = activityThreadClass.GetDeclaredField("mActivities");
            activityFields.Accessible = true;

            var obj = activityFields.Get(activityThread);

            if (obj is JavaDictionary)
            {
                var activities = (JavaDictionary)obj;
                objects = new List<Object>(activities.Values.Cast<Object>().ToList());
            }
            else if (obj is ArrayMap)
            {
                var activities = (ArrayMap)obj;
                objects = new List<Object>(activities.Values().Cast<Object>().ToList());
            }
            else if (obj is IMap)
            {
                var activities = (IMap)activityFields.Get(activityThread);
                objects = new List<Object>(activities.Values().Cast<Object>().ToList());
            }

            if (objects != null && objects.Any())
            {
                foreach (var activityRecord in objects)
                {
                    var activityRecordClass = activityRecord.Class;
                    var pausedField = activityRecordClass.GetDeclaredField("paused");
                    pausedField.Accessible = true;

                    if (!pausedField.GetBoolean(activityRecord))
                    {
                        var activityField = activityRecordClass.GetDeclaredField("activity");
                        activityField.Accessible = true;
                        activity = (Activity)activityField.Get(activityRecord);
                        break;
                    }
                }
            }

            return activity;
        }
    }
}