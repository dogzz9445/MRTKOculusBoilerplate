﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardSystem.Common
{
    public class FileGenerator
    {
        public static string GetUniqueName(string name, string folderPath, string extension)
        {
            string validatedName = name + extension;
            int tries = 1;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            while (File.Exists(folderPath + validatedName))
            {
                validatedName = string.Format("{0}_{1:00000}{2}", name, tries++, extension);
            }
            return validatedName;
        }
    }
}
