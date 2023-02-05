using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Threading;
using System.IO;

namespace LettoreXml
{
    internal class ImageResize
    {
        private readonly Config config;
        private string method;
        private string imgPath;
        private Image originalImage;
        private string selectedDimName;
        private int selectedWidth;
        private int selectedHeight;
        private bool selectedCrop;
        private float imgWidthOverHeight;
        private string outputFolderPath;
        private const string WIDTH = "width";
        private const string HEIGHT = "height";
        private const string CROP = "crop";

        public ImageResize(Config config)
        {
            this.config = config;
        }

        public string resize(string fileName, string dimName)
        {
            if (string.IsNullOrEmpty(dimName) || string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }
            loadOriginalImage(fileName);
            setDimensionsRatio();

        }

        private void loadOriginalImage(string imgName)
        {
            string sourceFolder = config.get("archive");

            if (!string.IsNullOrEmpty(sourceFolder))
            {
                string fullPath = Path.Combine(sourceFolder, imgName);
                originalImage = Image.FromFile(fullPath);
            }
            //TODO decidere cosa fare in caso di sourcefolder NULL
        }

        private void setDimensionsRatio()
        {
            imgWidthOverHeight = this.originalImage.Width/this.originalImage.Height;
        }


        private string getArchive()
        {
            dynamic retVal = config.get("archive");
            if (retVal != null)
            {
                return (string) retVal;
            }
            else
            {
                return null;
            }
        }

        private void setDimensionName(string dimName)
        {
            selectedDimName= dimName;
        }

        private void calculateDims()
        {
            dynamic width = getFullKeyMeasure(WIDTH);
            dynamic height = getFullKeyMeasure(HEIGHT);
            dynamic crop = getFullKeyMeasure(CROP);
            if(width!= null && height != null && crop != null)
            {
                selectedCrop = crop;
                if(dimensionIsNumeric(width))
                {
                    selectedWidth= width;
                    if(dimensionIsNumeric(height)) 
                    {
                        selectedHeight= height;
                    }
                    else
                    {
                        interpolateHeight();
                    }
                } else 
                {
                    selectedHeight= height;
                    interpolateWidth();
                }
            }
            else
            {
                //TODO decidere come gestire il caso di valori null
                throw new ArgumentNullException("width or height or crop is null");
            }
        }

        private void interpolateWidth()
        {
            selectedWidth = (int)Math.Round(selectedHeight * imgWidthOverHeight, 0);
        }

        private void interpolateHeight()
        {
            selectedHeight = (int)Math.Round(selectedWidth / imgWidthOverHeight,0);
        }

        //private bool isValidDimensionName()
        //{
        //    return getArchive
        //}

        private void setDimensionWidth()
        {
            selectedWidth = (int) config.get(selectedDimName + "/" + "width");
        }

        private void setDimensionHeight()
        {
            selectedHeight = (int) config.get(selectedHeight+ "/" + "height");
        }

        private bool isNullValueDimension(string dimName)
        {
            return config.get(selectedDimName + "/" + dimName) == null;
        }

        private bool dimensionIsNumeric(dynamic val)
        {
            return (val.GetType() == typeof(int));
        }

        private bool dimensionNotNumeric(dynamic val)
        {
            return (val.GetType() == typeof(string));
        }

        private int getNumericVal(string query)
        {
            dynamic val = config.get(selectedDimName + "/" + query);
            if (val != null)
            {
                return (int)val;
            }
            else
            {
                return -1;
            }
        }

        private dynamic getFullKeyMeasure(string query)
        {
            return config.get(selectedDimName + "/" + query);
        }
    }
}
