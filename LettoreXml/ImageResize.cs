using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO.Enumeration;
using System.Drawing.Drawing2D;
using System.Linq;

namespace LettoreXml
{
    internal class ImageResize
    {
        private readonly Config config;
        private string resizeDefinition;
        private string sourceImgName;
        private string imgPath;
        private Image originalImage;
        private Image resizedImage;
        private ImageFormat imageFormat;
        //private string selectedDimName;
        private dynamic dynSelectedWidth;
        private dynamic dynSelectedHeight;
        private int selectedWidth;
        private int selectedHeight;
        private bool selectedCrop;
        private float imgWidthOverHeight;
        private float originalOverResizedWidthRatio;
        private float originalOverResizedHeightRatio;
        private float chosenRatio;
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
            if (checksOk)
            {
                //Bitmap bitmap = new Bitmap(originalImage);
                //Size resizingDims = new Size(selectedWidth, selectedHeight);
                //resizedImage = (Image)new Bitmap(bitmap, resizingDims);

                string outputFileName = outputFolderPath + fileName;

                //using(var bmpImage = new Bitmap(originalImage))
                //{
                //    using (var bmpCrop = bmpImage.Clone(new Rectangle(0, 0, selectedWidth, selectedHeight), bmpImage.PixelFormat))
                //    {
                //        bmpCrop.Save(outputFileName);
                //    }
                //}

                //resizeImage(outputFileName);

                //resizedImage.Dispose();

                //createTestImg(outputFileName);

                resizeAndCropImageTest(outputFileName);
            }
        }

        private void resizeImage(string outputFileName)
        {
            using (var nb = new Bitmap(selectedWidth, selectedHeight))
            {
                using (Graphics graphics = Graphics.FromImage(nb))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(originalImage, 0, 0, selectedWidth, selectedHeight);

                    nb.Save(outputFileName);
                }
            }
        }

        //da ref. https://alex.domenici.net/archive/resize-and-crop-an-image-keeping-its-aspect-ratio-with-c-sharp
        private void createTestImg(string outputFileName)
        {
            using(var nb = new Bitmap(100, 100))
            {
                using (Graphics graphics = Graphics.FromImage(nb))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    //graphics.Clear(Color.White);
                    //graphics.DrawImage()
                    SolidBrush whiteBrush = new SolidBrush(Color.White);
                    Rectangle rect = new Rectangle(0,0,50,50);
                    graphics.FillRectangle(whiteBrush, rect);
                    nb.Save(outputFileName);
                }
            }
        }

        //da ref. https://alex.domenici.net/archive/resize-and-crop-an-image-keeping-its-aspect-ratio-with-c-sharp
        private void resizeAndCropImageTest(string outputFileName)
        {
            using (var target = new Bitmap(selectedWidth, selectedHeight))
            {
                using (Graphics graphics = Graphics.FromImage(target))
                {
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    fixImgOrientationIssues();

                    float scaling;
                    if (!selectedCrop)
                    {
                        scaling = originalOverResizedWidthRatio >= originalOverResizedHeightRatio ? originalOverResizedWidthRatio : originalOverResizedHeightRatio;
                    }
                    else
                    {
                        scaling = originalOverResizedWidthRatio <= originalOverResizedHeightRatio ? originalOverResizedWidthRatio : originalOverResizedHeightRatio;
                    }
                    int newWidth = (int)(originalImage.Width / scaling);
                    int newHeight = (int)(originalImage.Height / scaling);

                    int shiftX = (int)((newWidth - selectedWidth) / 2);
                    int shiftY = (int)((newHeight - selectedHeight) / 2);

                    graphics.DrawImage(originalImage, -shiftX, -shiftY, newWidth, newHeight);

                    new Bitmap(target).Save(outputFileName);
                }
            }
        }

        //ref. per questo metodo https://stackoverflow.com/questions/6222053/problem-reading-jpeg-metadata-orientation/23400751#23400751
        private void fixImgOrientationIssues()
        {
            if (Array.IndexOf(originalImage.PropertyIdList, 274) > -1)
            {
                var orientation = (int)originalImage.GetPropertyItem(274).Value[0];
                switch (orientation)
                {
                    case 1:
                        // No rotation required.
                        break;
                    case 2:
                        originalImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        originalImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        originalImage.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        originalImage.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        originalImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        originalImage.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        originalImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                // This EXIF data is now invalid and should be removed.
                originalImage.RemovePropertyItem(274);
            }
        }

        static void FixImageOrientation(Image srce)
        {
            const int ExifOrientationId = 0x112;
            // Read orientation tag
            if (!srce.PropertyIdList.Contains(ExifOrientationId)) return;
            var prop = srce.GetPropertyItem(ExifOrientationId);
            // Force value to 1
            prop.Value = BitConverter.GetBytes((short)1);
            srce.SetPropertyItem(prop);
        }

        private bool setChosenRatioOk()
        {
            if (selectedCrop)
            {
                chosenRatio = originalOverResizedWidthRatio <= originalOverResizedHeightRatio ? originalOverResizedWidthRatio : originalOverResizedHeightRatio;
            }
            else
            {
                chosenRatio = originalOverResizedWidthRatio >= originalOverResizedHeightRatio ? originalOverResizedWidthRatio : originalOverResizedHeightRatio;
            }
            return chosenRatio > 0;
        }

        private bool loadImageFormatOk()
        {
            List<ImageFormat> allowedFormats= new List<ImageFormat>() { ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Gif };
            imageFormat = originalImage.RawFormat;
            return allowedFormats.Contains(imageFormat);
        }

        private bool inputParamsOk(string fileName, string resizeDefinition)
        {
            return
                loadSourceImgNameOk(fileName)
                && loadResizeDefinitionOk(resizeDefinition)
                && loadArchiveRefOk()
                && loadCacheRefOk()
                && loadCropRefOk()
                && loadWidthRefOk()
                && loadHeightRefOk()
                && loadOriginalImageOk(fileName)
                && loadImageFormatOk()
                && setDimensionsRatioOk()
                && calculateOutputDimsOk()
                && setWidthRatioOk()
                && setHeightRatioOk()
                && setChosenRatioOk()
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
                if (selectedWidth <= 0)
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
                if (selectedHeight <= 0)
                    return false;
                return setInterpolatedWidth() > 0;
            }
        }

        private bool setWidthRatioOk()
        {
            originalOverResizedWidthRatio = (float)originalImage.Width / selectedWidth;
            return originalOverResizedWidthRatio> 0;
        }

        private bool setHeightRatioOk()
        {
            originalOverResizedHeightRatio = (float)(originalImage.Height / selectedHeight);
            return originalOverResizedHeightRatio> 0;
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
