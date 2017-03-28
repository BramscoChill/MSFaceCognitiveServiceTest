using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using AForge.Video;
using AForge.Video.DirectShow;
using ClientLibrary.Helpers;
using ClientLibrary.Model;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Face.Model;

namespace ClientLibrary.Controls
{
    /// <summary>
    /// Interaction logic for LiveFaceDetectionWindow.xaml
    /// </summary>
    public partial class LiveFaceDetectionWindow : Window
    {
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
        private int frameCounter = 0;
        private int detectionsSucceeded = 0;
        private int detectionsFailled = 0;

        //private FaceServiceClient faceServiceClient;
        private HaarObjectDetector detector;

        private ObservableCollection<ImageItem> imageItems = new ObservableCollection<ImageItem>();
        private ObservableCollection<string> persons = new ObservableCollection<string>();

        public ObservableCollection<string> Persons
        {
            get { return persons; }
            set
            {
                persons = value;
                OnPropertyChanged("Persons");
            }
        }

        public ObservableCollection<ImageItem> ImageItems
        {
            get { return imageItems; }
            set
            {
                imageItems = value;
                OnPropertyChanged("ImageItems");
            } 
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

        }

        public LiveFaceDetectionWindow()
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
            Unloaded += Window_Unload;

            foreach (string person in faceHelper.database.GetPersons().Select(p => p.Name))
            {
                Persons.Add(person);
            }
            cmbPersons.ItemsSource = persons;
            cmbPersons.SelectionChanged += CmbPersonsOnSelectionChanged;
        }

        private void CmbPersonsOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            //mind the differencde between the Text and SelectedValue
            string personName = string.IsNullOrEmpty(cmbPersons.SelectedValue.ToString()) ? cmbPersons.Text : cmbPersons.SelectedValue.ToString();

            //gets all the person image names from the DB
            var faces = faceHelper.database.GetFacesFromPersonName(personName);
            lblPersonImageCount.Content = $"{faces.Count}";
            if (faces != null && faces.Count > 0)
            {
                ImageItems.Clear();
                foreach (FaceExtended face in faces)
                {
                    ImageItems.Add(new ImageItem() { FileName = Path.GetFileName(face.ImageName), FullPath = face.ImageName, FaceId = face.FaceId});
                }
            }
        }

        private void Window_Unload(object sender, RoutedEventArgs e)
        {
            ExitApp();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoaclWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            LocalWebCam = new VideoCaptureDevice(LoaclWebCamsCollection[0].MonikerString);
            LocalWebCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);

            LocalWebCam.Start();
            processingStopwatch.Start();

            lstImages.ItemsSource = imageItems;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            ExitApp();
        }

        
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

               

                //slows it down
//                if (faces != null && faces.Length > 0)
//                {
//                    Dispatcher.BeginInvoke(new ThreadStart(delegate
//                    {
//                        Rectangle rect = faces.FirstOrDefault();
//                        rectFace.Width = rect.Width;
//                        rectFace.Height = rect.Height;
//                        rectFace.Margin = new Thickness(frameHolder.Margin.Left, frameHolder.Margin.Top,
//                            frameHolder.Margin.Right, frameHolder.Margin.Bottom);
//                    }));
//                }

                if (isProcessing == false && frameCounter % 5 == 0 && processingStopwatch.ElapsedMilliseconds > 3000)
                {
                    try
                    {
                        //slows it down if executed on each frame
                        faces = detector.ProcessFrame((Bitmap)eventArgs.Frame.Clone());
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
                            processingStopwatch.Start();
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



                var g = Graphics.FromImage(img);
                
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
            string foundPerson = faceHelper.DetectFace(fullFilePath);
            if (foundPerson != null)
            {
                detectionsSucceeded++;
            }
            else
            {
                detectionsFailled++;
            }

            Application.Current.Dispatcher.Invoke((Action) delegate
            {
                //DetectedResultsInText = foundList;
                if (foundPerson != null)
                {
                    lblInfo.Content = foundPerson;
                }
                lblNumberOfTries.Content =
                    $"Decetions: {detectionsFailled + detectionsSucceeded}, failled: {detectionsFailled}, succeeded: {detectionsSucceeded}";
            });
        }

        private void ButtonTakePicture_OnClick(object sender, RoutedEventArgs e)
        {
            string fullFilePath = SaveCurrentFrame();
            ImageItems.Add(new ImageItem() {FileName = Path.GetFileName(fullFilePath), FullPath = fullFilePath});
            
            //            ListBoxItem lbi = new ListBoxItem();
            //            lbi.Content = Path.GetFileName(fullFilePath);
            //            lbi.SetValue(new DependencyProperty(), );
            //            FileList
            //lstImages.Items.Add(lbi);
        }

        private void ButtonAddAsPerson_OnClick(object sender, RoutedEventArgs e)
        {
            int amountUpdated = faceHelper.AddPerson(cmbPersons.Text, ImageItems.ToList());
            MessageBox.Show($"Finished! - amount added / updated {amountUpdated}");
        }

        private void ExitApp()
        {
            LocalWebCam.Stop();
            processingStopwatch.Stop();
            Environment.Exit(0);
        }
        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            ImageItems.Clear();
        }

        private void ButtonTest_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void ButtonClearInfo_OnClick(object sender, RoutedEventArgs e)
        {
            lblInfo.Content = "-";
            detectionsSucceeded = 0;
            detectionsFailled = 0;
        }
    }
}
