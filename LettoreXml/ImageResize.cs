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
        private Image originalImage;
        private ImageFormat imageFormat;
        private dynamic dynSelectedWidth;
        private dynamic dynSelectedHeight;
        private int selectedWidth;
        private int selectedHeight;
        private bool selectedCrop;
        private string[] selectedFilters;
        private float imgWidthOverHeight;
        private float originalOverResizedWidthRatio;
        private float originalOverResizedHeightRatio;
        private string inputFolderPath;
        private string outputFolderPath;
        private const string WIDTH = "width";
        private const string HEIGHT = "height";
        private const string CROP = "crop";
        private const string CACHE = "imageCache";
        private const string ARCHIVE = "archive";
        private const string FILTERS = "filters";

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
                    resizeCropApplyFiltersToImage(outputFileName);
                    upsertResizeDate();
                }
            }
            flushInputParams();
        }

        private void resizeCropApplyFiltersToImage(string outputFileName)
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

                    applyFilters();

                    target.Save(outputFileName);
                }
            }
        }

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

        private void applyFilters()
        {
            if (selectedFilters != null)
            {
                foreach (var filter in selectedFilters)
                {
                    //not implemented
                }
            }
        }
        
        private void flushInputParams()
        {
            resizeDefinition = null;
            sourceImgName= null;
            if (originalImage != null)
            {
                originalImage.Dispose();
            }
            imageFormat = null;
            dynSelectedWidth = null;
            dynSelectedHeight = null;
            selectedWidth = 0; 
            selectedHeight = 0;
            selectedCrop = false;
            selectedFilters = null;
            imgWidthOverHeight = 0;
            originalOverResizedWidthRatio = 0;
            originalOverResizedHeightRatio= 0;
            inputFolderPath= null;
            outputFolderPath= null;
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
                && loadFiltersOk()
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
            return (imgWidthOverHeight = (float)originalImage.Width / originalImage.Height) > 0;
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
        
        private bool loadFiltersOk()
        {
            dynamic filters = getFullKeyMeasure(FILTERS);
            if (filters != null)
            {
                selectedFilters= filters;
            }
            return true;
        }
        #endregion

        #region Caching System
        private MemoryCache imagesCache = new MemoryCache("ImagesCache");
        private CacheItemPolicy cacheItemPolicy = new CacheItemPolicy() { AbsoluteExpiration = DateTime.MaxValue };

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

        private string generateImagesCacheKey()
        {
            return string.Concat(sourceImgName, "_", resizeDefinition);
        }
        #endregion
    }
}
