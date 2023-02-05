using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace LettoreXml
{
    internal class ImageResize
    {
        private readonly Config config;
        private string resizeDefinition;
        private string sourceImgName;
        private string imgPath;
        private Image originalImage;
        //private string selectedDimName;
        private dynamic dynSelectedWidth;
        private dynamic dynSelectedHeight;
        private int selectedWidth;
        private int selectedHeight;
        private bool selectedCrop;
        private float imgWidthOverHeight;
        private string inputFolderPath;
        private string outputFolderPath;
        private const string WIDTH = "width";
        private const string HEIGHT = "height";
        private const string CROP = "crop";
        private const string CACHE = "imageCache";
        private const string ARCHIVE = "archive";

        public ImageResize(Config config)
        {
            this.config = config;
        }

        public void resize(string fileName, string resizeDefinition)
        {
            bool checksOk = inputParamsOk(fileName, resizeDefinition);
        }

        private bool inputParamsOk(string fileName, string resizeDefinition)
        {
            return
                loadSourceImgNameOk(fileName)
                && loadResizeDefinitionOk(resizeDefinition)
                && !string.IsNullOrEmpty(resizeDefinition)
                && loadArchiveRefOk()
                && loadCacheRefOk()
                && loadCropRefOk()
                && loadWidthRefOk()
                && loadHeightRefOk()
                && loadOriginalImageOk(fileName)
                && setDimensionsRatioOk()
                && calculateOutputDimsOk()
                ;
        }

        private bool loadSourceImgNameOk(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                sourceImgName = fileName;
                return true;
            } return false;
        }

        private bool loadResizeDefinitionOk(string resizeDefinition)
        {
            if (!string.IsNullOrEmpty(resizeDefinition))
            {
                this.resizeDefinition = resizeDefinition;
                return true;
            }
            return false;
        }

        private bool loadArchiveRefOk()
        {
            string sourceFolder;
            if((sourceFolder = config.get(ARCHIVE)) != null)
            {
                inputFolderPath= sourceFolder;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool loadCacheRefOk()
        {
            string destFolder;
            if((destFolder = config.get(CACHE))!= null)
            {
                outputFolderPath= destFolder;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool loadCropRefOk()
        {
            dynamic checkCrop = getFullKeyMeasure(CROP);
            if (checkCrop != null)
            {
                selectedCrop = checkCrop;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool loadWidthRefOk()
        {
            dynamic checkWidth= getFullKeyMeasure(WIDTH);
            if(checkWidth != null)
            {
                dynSelectedWidth = checkWidth;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool loadHeightRefOk()
        {
            dynamic checkHeight = getFullKeyMeasure(HEIGHT);
            if(checkHeight != null)
            {
                dynSelectedHeight = checkHeight;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool calculateOutputDimsOk()
        {
            if (dimensionIsNumeric(dynSelectedWidth))
            {
                selectedWidth = dynSelectedWidth;
                if (selectedWidth == 0)
                {
                    return false;
                }
                if (dimensionIsNumeric(dynSelectedHeight))
                {
                    selectedHeight= dynSelectedHeight;
                    return selectedHeight > 0;
                }
                else
                {
                    return setInterpolatedHeight() > 0;
                }
            }
            else
            {
                selectedHeight = dynSelectedHeight;
                if (selectedHeight == 0)
                    return false;
                return setInterpolatedWidth() > 0;
            }
        }

        private bool loadOriginalImageOk(string imgName)
        {
            string fullPath = Path.Combine(inputFolderPath, imgName);
            if (File.Exists(fullPath))
            {
                originalImage = Image.FromFile(fullPath);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        private bool setDimensionsRatioOk()
        {
            return (imgWidthOverHeight = (float)this.originalImage.Width / this.originalImage.Height) > 0;
        }

        private bool dimensionIsNumeric(dynamic val)
        {
            return (val.GetType() == typeof(int));
        }

        private int setInterpolatedWidth()
        {
            selectedWidth = (int)Math.Round(selectedHeight * imgWidthOverHeight, 0);
            return selectedWidth;
        }

        private int setInterpolatedHeight()
        {
            selectedHeight = (int)Math.Round(selectedWidth / imgWidthOverHeight, 0);
            return selectedHeight;
        }

        private dynamic getFullKeyMeasure(string query)
        {
            return config.get(resizeDefinition + "/" + query);
        }



        //private void setDimensionName(string dimName)
        //{
        //    selectedDimName= dimName;
        //}

        //private void setDimensionWidth()
        //{
        //    selectedWidth = (int) config.get(selectedDimName + "/" + "width");
        //}

        //private void setDimensionHeight()
        //{
        //    selectedHeight = (int) config.get(selectedHeight+ "/" + "height");
        //}

        //private bool isNullValueDimension(string dimName)
        //{
        //    return config.get(selectedDimName + "/" + dimName) == null;
        //}

        //private bool dimensionNotNumeric(dynamic val)
        //{
        //    return (val.GetType() == typeof(string));
        //}

        //private int getNumericVal(string query)
        //{
        //    dynamic val = config.get(selectedDimName + "/" + query);
        //    if (val != null)
        //    {
        //        return (int)val;
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //private string getArchive()
        //{
        //    dynamic retVal = config.get("archive");
        //    if (retVal != null)
        //    {
        //        return (string) retVal;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
    }
}
