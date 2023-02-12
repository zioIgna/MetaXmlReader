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
using System.Runtime.Caching;
using System.Runtime.Serialization;

namespace LettoreXml
{
    internal class ImageResize
    {
        private readonly Config config;
        private string resizeDefinition;
        private string sourceImgName;
        //private string imgPath;
        private Image originalImage;
        //private Image resizedImage;
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
                string outputFileName = outputFolderPath + fileName;
                if (imgNeedsEditing())
                {
                    resizeAndCropImage(outputFileName);
                    upsertResizeDate();
                }
                //addTupleKey3(outputFileName);
                //bool success = addTupleKey2(fileName, resizeDefinition);
                //if (success)
                //{
                //    DateTime editTime = (DateTime)imagesCache.Get(string.Concat(fileName, "_", resizeDefinition));
                //}
            }
            if (originalImage != null)
            {
                originalImage.Dispose();
            }
        }

        //da ref. https://alex.domenici.net/archive/resize-and-crop-an-image-keeping-its-aspect-ratio-with-c-sharp
        private void resizeAndCropImage(string outputFileName)
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

                    using (var result = new Bitmap(target))
                    {
                        result.Save(outputFileName);
                    }
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

        #region Input Params Check
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
        
        private bool loadImageFormatOk()
        {
            List<ImageFormat> allowedFormats= new List<ImageFormat>() { ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Gif };
            imageFormat = originalImage.RawFormat;
            return allowedFormats.Contains(imageFormat);
        }
        
        private bool setDimensionsRatioOk()
        {
            return (imgWidthOverHeight = (float)this.originalImage.Width / this.originalImage.Height) > 0;
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
        #endregion

        #region Caching System
        private MemoryCache imagesCache = new MemoryCache("ImagesCache");
        private CacheItemPolicy cacheItemPolicy = new CacheItemPolicy() { AbsoluteExpiration = DateTime.MaxValue };

        private bool addTupleKey(string imgName, string resizeDefinition)
        {
            string key = generateImagesCacheKey();
            string extendedDateStr = Encoding.UTF8.GetString(originalImage.GetPropertyItem(0x0132).Value);
            originalImage.GetPropertyItem(0x0132).Value = System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString());
            //originalImage.SetPropertyItem(originalImage.GetPropertyItem(0x0132));
            string strippedDateStr = extendedDateStr.Substring(0, (extendedDateStr.IndexOf('\\')));  //extendedDateStr.Length - (extendedDateStr.Length -
            DateTime srcImgEditTime = DateTime.ParseExact(strippedDateStr, "yyyy:MM:dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            CacheItem cacheItem = new CacheItem(key, srcImgEditTime);
            return imagesCache.Add(cacheItem, cacheItemPolicy);
        }

        private bool addTupleKey2(string imgName, string resizeDefinition)
        {
            string extendedDateStr = Encoding.UTF8.GetString(originalImage.GetPropertyItem(0x0132).Value);
            int index = extendedDateStr.LastIndexOf(':') +3;
            string strippedDateSTr = extendedDateStr.Substring(0,index);
            DateTime srcImgEditTime = DateTime.ParseExact(strippedDateSTr,"yyyy:MM:dd HH:mm:ss",null);
            return true;
        }

        private void addTupleKey3(string outputFileName)
        {
            if (File.Exists(outputFileName))
            {
                Console.WriteLine(File.GetLastWriteTime(outputFileName).ToString());
            }
        }

        private bool imgNeedsEditing()
        {
            string imgToCheck = getOriginalImgFullPath();
            string key = generateImagesCacheKey();
            if (!imagesCache.Contains(key))
            {
                return true;
            }
            else
            {
                DateTime cachedDate = (DateTime)imagesCache.Get(key);
                DateTime fileSystemDate = File.GetLastWriteTime(imgToCheck);
                return DateTime.Compare(cachedDate, fileSystemDate) < 0;
            }
        }

        private void upsertResizeDate()
        {
            var cacheItem = new CacheItem(generateImagesCacheKey(), File.GetLastWriteTime(getOriginalImgFullPath()));
            imagesCache.Set(cacheItem, cacheItemPolicy);
        }

        private string getOriginalImgFullPath()
        {
            return inputFolderPath + sourceImgName;
        }

        private bool imgHasEditDate()
        {
            DateTime dateTimeToSet = DateTime.Now;
            try
            {
                Encoding.UTF8.GetString(originalImage.GetPropertyItem(0x0132).Value);
                return true;
            }
            catch (ArgumentException e)
            {
                //byte[] imgDateByteArr = BitConverter.GetBytes(dateTimeToSet.Ticks);
                //string byteArrToStr = Encoding.UTF8.GetString((byte[])imgDateByteArr);
                return false;
            }
        }

        private void compareImgEditDate()
        {
            if(!imgHasEditDate())
            {
                byte[] imgDateByteArr = BitConverter.GetBytes(DateTime.Now.Ticks);
                string byteArrToStr = Encoding.UTF8.GetString((byte[])imgDateByteArr);
                originalImage.GetPropertyItem(0x0132).Value = imgDateByteArr;
                //originalImage.SetPropertyItem(0x0132).Value
            }
        }

        private string generateImagesCacheKey()
        {
            return string.Concat(sourceImgName, "_", resizeDefinition);
        }

        //private bool imgNeedsProcessing(string fileName, string resizeDefinition)
        //{
        //    //ValueTuple<string, string> key
        //    //return imagesCache.TryGetValue(fileName, out cacheItemPolicy);

        //}

        #endregion
    }
}
