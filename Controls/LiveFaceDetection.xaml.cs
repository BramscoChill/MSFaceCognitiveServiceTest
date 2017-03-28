//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Microsoft Cognitive Services (formerly Project Oxford): https://www.microsoft.com/cognitive-services
//
// Microsoft Cognitive Services (formerly Project Oxford) GitHub:
// https://github.com/Microsoft/Cognitive-Face-Windows
//
// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;
using System.Linq;
using ClientLibrary.Controls;
using System.Threading.Tasks;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using ClientLibrary.Helpers;
using Microsoft.ProjectOxford.Face.Contract;

namespace Microsoft.ProjectOxford.Face.Controls
{
    /// <summary>
    /// Interaction logic for FaceDetectionPage.xaml
    /// </summary>
    public partial class LiveFaceDetectionPage : Page, INotifyPropertyChanged
    {

        #region Fields

        /// <summary>
        /// Description dependency property
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(FaceDetectionPage));

        /// <summary>
        /// Face detection results in list container
        /// </summary>
        private ObservableCollection<Face> _detectedFaces = new ObservableCollection<Face>();

        /// <summary>
        /// Face detection results in text string
        /// </summary>
        private string _detectedResultsInText;

        /// <summary>
        /// Face detection results container
        /// </summary>
        private ObservableCollection<Face> _resultCollection = new ObservableCollection<Face>();

        private FaceHelper faceHelper;
        private string _selectedFile;
        VideoCaptureDevice LocalWebCam;
        public FilterInfoCollection LoaclWebCamsCollection;
        private System.Timers.Timer faceDetectionIntervalTimer;
        //private MemoryStream currentMsImage;
        private Bitmap currentFrame;
        private object lockObject = new object();
        private volatile bool isProcessing = false;
        private Stopwatch processingStopwatch = new Stopwatch();

        private FaceServiceClient faceServiceClient;
        private HaarObjectDetector detector;

        //public static readonly string GroupName = "TestPeople";
        public string GroupName = "TestPeople";
        public string GroupID= Guid.NewGuid().ToString();

        
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceDetectionPage" /> class
        /// </summary>
        public LiveFaceDetectionPage()
        {
            InitializeComponent();

            faceHelper = new FaceHelper();
            FaceHaarCascade cascade = new FaceHaarCascade();
            detector = new HaarObjectDetector(cascade, 30);
            detector.SearchMode = ObjectDetectorSearchMode.Average;
            detector.ScalingFactor = 1.5f;
            detector.ScalingMode = ObjectDetectorScalingMode.GreaterToSmaller;
            detector.UseParallelProcessing = true;
            detector.Suppression = 3;

            Loaded += MainWindow_Loaded;
            Unloaded += delegate(object sender, RoutedEventArgs args) { Environment.Exit(0); };
            
            
            //faceHelper.TEST();

            

//
//            string subscriptionKey = Constants.FACE_SUBSCRIPTION_KEY;
//
//            faceServiceClient = new FaceServiceClient(subscriptionKey);
//
//

//            faceDetectionIntervalTimer = new System.Timers.Timer()
//            {
//                AutoReset = true,
//                Interval = 4000,
//                Enabled = false,
//            };
//            faceDetectionIntervalTimer.Elapsed += faceDetectionIntervalTimer_Elapsed;
//
//
//            
//               Thread thread = new Thread(new ThreadStart(delegate ()
//               {
//                   try
//                   {
//                       var groupTask = faceServiceClient.ListPersonGroupsAsync();
//                       groupTask.Wait();
//                       var groupsResult = groupTask.Result;
//                       bool foundGroup = false;
//                       foreach (var group in groupsResult)
//                       {
//                           if (group.Name == GroupName)
//                           {
//                               foundGroup = true;
//                               GroupID = group.PersonGroupId;
//                               break;
//                           }
//                       }
//                       if (!foundGroup)
//                       {
//                           faceServiceClient.CreatePersonGroupAsync(GroupID, GroupName).Wait();
//                       }
//
//                       faceDetectionIntervalTimer.Enabled = true;
//                   }
//                   catch (Exception ex)
//                   {
//
//                   }
//
//               }));
//               thread.Start();


        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Implement INotifyPropertyChanged event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets description
        /// </summary>
        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }

            set
            {
                SetValue(DescriptionProperty, value);
            }
        }

        /// <summary>
        /// Gets face detection results
        /// </summary>
        public ObservableCollection<Face> DetectedFaces
        {
            get
            {
                return _detectedFaces;
            }
        }

        /// <summary>
        /// Gets or sets face detection results in text string
        /// </summary>
        public string DetectedResultsInText
        {
            get
            {
                return _detectedResultsInText;
            }

            set
            {
                _detectedResultsInText = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DetectedResultsInText"));
                }
            }
        }

        /// <summary>
        /// Gets constant maximum image size for rendering detection result
        /// </summary>
        public int MaxImageSize
        {
            get
            {
                return 300;
            }
        }

        /// <summary>
        /// Gets face detection results
        /// </summary>
        public ObservableCollection<Face> ResultCollection
        {
            get
            {
                return _resultCollection;
            }
        }

        /// <summary>
        /// Gets or sets image path for rendering and detecting
        /// </summary>
        public string SelectedFile
        {
            get
            {
                return _selectedFile;
            }

            set
            {
                _selectedFile = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedFile"));
                }
            }
        }

        #endregion Properties

        #region Methods
        
        

    void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        LoaclWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        LocalWebCam = new VideoCaptureDevice(LoaclWebCamsCollection[0].MonikerString);
        LocalWebCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);

        LocalWebCam.Start();
        processingStopwatch.Start();
    }

        private int frameCounter = 0;
    private void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Rectangle[] faces = null;
                lock (lockObject)
                {
                    currentFrame = (Bitmap)eventArgs.Frame.Clone();

                }

                //faces = detector.ProcessFrame((Bitmap)eventArgs.Frame.Clone());

                System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone();

                if (isProcessing == false && frameCounter % 5 == 0 && processingStopwatch.ElapsedMilliseconds > 3000)
                {
                    try
                    {
                        faces = detector.ProcessFrame(currentFrame);
                    }
                    catch (System.AccessViolationException e)
                    {
                        
                    }
                    catch (Exception e)
                    {

                    }

                    if (faces != null && faces.Length > 0)
                    {
                        isProcessing = true;
                        string fullFilePath = SaveCurrentFrame();
                        new Thread(() =>
                        {
                            TryDetectFace(fullFilePath);
                            processingStopwatch.Reset();
                            isProcessing = false;
                        }).Start();
                        //                        Graphics g = Graphics.FromImage(img);
                        //                        foreach (Rectangle face in faces)
                        //                        {
                        //                            g.DrawRectangle(Pens.DeepSkyBlue, face);
                        //                        }
                        //                        g.Dispose();
                    }
                    frameCounter = 0;
                }



                Graphics.FromImage(img);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();

                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);


                //currentMsImage = new MemoryStream();
                //var currentMsImage = new MemoryStream();
                //this.currentMsImage = new MemoryStream();
                //img.Save(currentMsImage, ImageFormat.Bmp);
                //img.Save(currentMsImage, ImageFormat.Jpeg);
                //currentMsImage.CopyTo(this.currentMsImage);
                //currentMsImage.Seek(0, SeekOrigin.Begin);

                bi.StreamSource = ms;
                bi.EndInit();

                bi.Freeze();

                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    frameHolder.Source = bi;
                }));

                frameCounter++;
            }
            catch (Exception ex)
            {
            }
        }
        

//        private async void faceDetectionIntervalTimer_Elapsed(object sender, ElapsedEventArgs e)
//        {
//            //var tag = System.IO.Path.GetFileName(dir);
//            Person p = new Person();
//            //p.PersonName = tag;
//            p.PersonName = Guid.NewGuid().ToString();
//
//            var faces = new ObservableCollection<Face>();
//            p.Faces = faces;
//            //p.PersonId = (faceServiceClient.CreatePersonAsync(GroupID, p.PersonName)).PersonId.ToString();
//
//            //this.currentMsImage.Seek(0, SeekOrigin.Begin);
//
//            //currentMsImage.WriteTo
//            //using (var fileStream = File.OpenRead(@"C:\tmp.jpg"))
//
//            string fileNamePath = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString() + ".jpg");
//            
//            try
//            {
//                var fileStream = new FileStream(fileNamePath, FileMode.OpenOrCreate);
//
//                currentMsImage.WriteTo(fileStream);
//                fileStream.Seek(0, SeekOrigin.Begin);
//
//                //byte[] bytes = new byte[fileStream.Length];
//                //fileStream.Read(bytes, 0, (int)fileStream.Length);
//                //ms.Write(bytes, 0, (int)fileStream.Length);
//
//
//                Contract.Face[] faceDetectResult = faceServiceClient.DetectAsync(fileStream, true, false).Result;
//                fileStream.Dispose();
//
//                var personNameId = faceDetectResult.First().FaceId;
//                var createdPerson = faceServiceClient.CreatePersonAsync(GroupID, personNameId.ToString()).Result;
//                
//                fileStream = new FileStream(fileNamePath, FileMode.OpenOrCreate);
//                currentMsImage.WriteTo(fileStream);
//                fileStream.Seek(0, SeekOrigin.Begin);
//
//                var persFaceResult = faceServiceClient.AddPersonFaceAsync(GroupID, createdPerson.PersonId, fileStream).Result;
//
//                
//                fileStream.Dispose();
//
//
//                //foreach (Contract.Face face in faceDetectResult)
//                //{
//                bool faceFound = false;
//                this.Dispatcher.Invoke(() =>
//                {
//                    foreach (ListBoxItem listboxItem in foundIdsListbox.Items)
//                    {
//                        if (listboxItem.Content.ToString().Equals(persFaceResult.PersistedFaceId.ToString(), StringComparison.OrdinalIgnoreCase))
//                        {
//                            faceFound = true;
//                            break;
//                        }
//                    }
//                });
//                    
//
//                if (!faceFound)
//                {
//                    Application.Current.Dispatcher.Invoke((Action)delegate {
//                        ListBoxItem lbi = new ListBoxItem();
//                        lbi.Content = persFaceResult.PersistedFaceId.ToString();
//                        foundIdsListbox.Items.Add(lbi);
//                    });
//                }
//
//                Application.Current.Dispatcher.Invoke((Action)delegate {
//                    //DetectedResultsInText = string.Join(" - ", faceDetectResult.Select(f => f.FaceId));
//                    DetectedResultsInText = persFaceResult.PersistedFaceId.ToString();
//                });
//            } catch (Exception ex)
//            {
//
//            }
//
//                
//
//                
//                //var faceDetectResult = faceServiceClient.DetectAsync(ms, true, false).Result;
//            
//
//            
//
//            File.Delete(fileNamePath);
//                
//            //var faceDetectResult = faceServiceClient.DetectAsync(currentMsImage, true, false).Result;
//
//            
//
//            //DetectedResultsInText = persFaceResult.PersistedFaceId.ToString();
//        }

        private string SaveCurrentFrame()
        {
            string fullFilePath = Path.Combine(Constants.TMP_IMAGE_DIR, Guid.NewGuid().ToString() + ".jpg");


            //locking the bitmap and copies it
            lock (lockObject)
            {
                Bitmap demBitmap = (Bitmap)currentFrame.Clone();
                demBitmap.Save(fullFilePath);
            }

            return fullFilePath;
        }

        private void TryDetectFace(string fullFilePath)
        {
            string foundList = faceHelper.DetectFace(fullFilePath);
            if (foundList != null)
            {
                Application.Current.Dispatcher.Invoke((Action) delegate {
                    DetectedResultsInText = foundList;
                });
            }
        }

        private async void ImagePicker_Click(object sender, RoutedEventArgs e)
        {
            //TryDetectFace();
        }

        #endregion Methods
    }
}