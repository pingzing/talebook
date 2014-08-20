using BirthdayNotifier.Common;
using BirthdayNotifier.Helper_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace BirthdayNotifier
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class LayoutResponsiveMainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public int TotalPages { get; set; }
        public List<int> ImagePages { get; set; }
        private Dictionary<int, Image> PageImageDictionary { get; set; }
        private List<Image> ImageList = new List<Image>();
        bool debugging = false;
        enum eColors { Purple, Gray };
        private eColors CurrentColor { get; set; }
        private DateTime UnlockDate = new DateTime(2014, 1, 13);

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public LayoutResponsiveMainPage()
        {
#if DEBUG
            debugging = true;
#endif
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            CurrentColor = eColors.Purple;
            CreateAndPrepareToast();
            PageImageDictionary = new Dictionary<int, Image>();
            DefineStrings("01");
            DefineImagePages("01");
            LoadImages("01");

        }

        private void DefineImagePages(string storyNumber)
        {
            ImagePages = Stories.GetImagePages(storyNumber);
        }

        private void DefineStrings(string storyNumber)
        {
            List<string> stringList = new List<string>();
            stringList = Stories.GetStoryText(storyNumber);
            List<TextBlock> blockList = new List<TextBlock>();

            switch (storyNumber)
            {
                case "01": //Snowbird                                                            
                    TotalPages = stringList.Count;
                    foreach (string str in stringList)
                    {
                        TextBlock block = new TextBlock();
                        string textString = str;
                        if (textString.Contains("<i>"))
                        {
                            textString = textString.Substring(textString.IndexOf(">") + 1, (textString.Length - 3));
                            block.Style = (Style)Resources["ItalicApplicationBlockStyle"];
                        }
                        else
                        {
                            block.Style = (Style)Resources["ApplicationBlockStyle"];
                        }

                        block.Text = textString;
                        block.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                        block.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                        blockList.Add(block);
                    }
                    TextFlipView.ItemsSource = blockList;
                    break;
            }
        }

        private void CreateAndPrepareToast()
        {
            DateTime deliveryTime = new DateTime(2014, 1, 13, 10, 0, 0);
            DateTime deliveryTime2 = new DateTime(2014, 1, 16, 10, 0, 0);
            var toastTemplate = ToastTemplateType.ToastImageAndText02;
            var toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            var strings = toastXml.GetElementsByTagName("text");
            var images = toastXml.GetElementsByTagName("image");
            strings[0].AppendChild(toastXml.CreateTextNode("Happy Birthday!"));
            strings[1].AppendChild(toastXml.CreateTextNode(("Happy birthday! Time to open your present!")));
            ((XmlElement)images[0]).SetAttribute("src", "ms-appx:///Assets/Logo.png");
            ((XmlElement)images[0]).SetAttribute("alt", "Logo");
            if (deliveryTime > DateTime.Now)
            {
                var toast = new ScheduledToastNotification(toastXml, deliveryTime);
                string id = Guid.NewGuid().ToString().Substring(0, 8);
                toast.Id = id;

                var toastNotifier = ToastNotificationManager.CreateToastNotifier();
                toastNotifier.AddToSchedule(toast);
            }
            if (deliveryTime2 > DateTime.Now)
            {
                var toast = new ScheduledToastNotification(toastXml, deliveryTime2);
                string id = Guid.NewGuid().ToString().Substring(0, 8);
                toast.Id = id;

                var toastNotifier = ToastNotificationManager.CreateToastNotifier();
                toastNotifier.AddToSchedule(toast);
            }
        }

        private async void LoadImages(string folderNumber)
        {
            string imagesPath = @"Assets\StoryImages" + folderNumber;
            StorageFolder installedFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder imagesFolder = await installedFolder.GetFolderAsync(imagesPath);
            IReadOnlyList<StorageFile> imageList = await imagesFolder.GetFilesAsync();
            foreach (var img in imageList)
            {
                BitmapImage bitmapImage = new BitmapImage();
                FileRandomAccessStream stream = (FileRandomAccessStream)await img.OpenAsync(FileAccessMode.Read);
                bitmapImage.SetSource(stream);
                Image newImage = new Image();
                newImage.Source = bitmapImage;
                ImageList.Add(newImage);
            }
            System.Diagnostics.Debug.WriteLine("Done loading images!");
            ImageBox.Child = ImageList[TextFlipView.SelectedIndex];

            int i = 0;
            foreach (int page in ImagePages)
            {
                PageImageDictionary.Add(page, ImageList[i]);
                i++;
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            if (TryRetrieveAuthorization())
            {
                backButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                backButton.Visibility = Visibility.Visible;
                ImageBox.Visibility = Visibility.Collapsed;
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void TextFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Handle image and page number changing
            int count = TextFlipView.Items.Count;
            int currentPage = TextFlipView.SelectedIndex + 1;
            PageBlock.Text = "Page: " + currentPage + "/" + TotalPages;
            if (TextFlipView.SelectedIndex != -1)
            {
                if (ImageList != null && PageImageDictionary != null)
                {
                    if (PageImageDictionary.ContainsKey(TextFlipView.SelectedIndex))
                    {
                        Image temp;
                        PageImageDictionary.TryGetValue(TextFlipView.SelectedIndex, out temp);
                        ImageBox.Child = temp;
                    }
                }
            }

            //Display "more to come"if it's before the 13th.
            if (DateTime.Now <= UnlockDate && !debugging)
            {
                if (currentPage >= 5)
                {
                    Image temp = new Image();
                    PageImageDictionary.TryGetValue(72, out temp);
                    ImageBox.Child = temp;

                    ((TextBlock)TextFlipView.SelectedItem).Text = "This page will unlock on " + UnlockDate + "...";
                }
            }
            else
            {
                //Handle background color and Title changing
                SolidColorBrush purpleBrush = new SolidColorBrush(Colors.Purple);
                if (currentPage >= 5 && CurrentColor == eColors.Purple)
                {
                    var myStoryboard = (Storyboard)Resources["PurpleToGrayBoard"];
                    myStoryboard.Stop();
                    foreach (var animation in myStoryboard.Children)
                    {
                        Storyboard.SetTarget(animation, MainPanel);
                        CurrentColor = eColors.Gray;
                    }
                    myStoryboard.Begin();
                    pageTitle.Text = "Snowbird";
                }
                else if (currentPage < 5 && CurrentColor == eColors.Gray)
                {
                    var myStoryboard = (Storyboard)Resources["GrayToPurpleBoard"];
                    myStoryboard.Stop();
                    foreach (var animation in myStoryboard.Children)
                    {
                        Storyboard.SetTarget(animation, MainPanel);
                        CurrentColor = eColors.Purple;
                    }
                    myStoryboard.Stop();
                    myStoryboard.Begin();
                    pageTitle.Text = "Happy Birthday";
                }
            }
        }

        private bool TryRetrieveAuthorization()
        {
            try
            {
                var applicationData = Windows.Storage.ApplicationData.Current;
                if (applicationData.LocalSettings.Values["isallowed"] != null)
                {
                    return (bool)applicationData.LocalSettings.Values["isallowed"];
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private void pageTitle_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        ApplicationViewOrientation? orientation;
        Double applicationWidth;
        private void pageRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();

            ApplicationViewOrientation newOrientation = view.Orientation;
            Double newWidth = Window.Current.Bounds.Width;
            bool shrunk = newWidth > applicationWidth;
            bool grew = newWidth < applicationWidth;

            if ((this.orientation == null) || (this.orientation != newOrientation) || shrunk || grew)
            {
                if (newOrientation == ApplicationViewOrientation.Landscape || (newOrientation != ApplicationViewOrientation.Portrait && newWidth >= 700))
                {
                    Column1.Width = new GridLength(Window.Current.Bounds.Width / 2);
                    Column2.Width = new GridLength(Window.Current.Bounds.Width / 2);
                    Row1.Height = new GridLength(Window.Current.Bounds.Height * 2 / 3);
                    Row2.Height = new GridLength(0);

                    TextFlipView.SetValue(Grid.ColumnProperty, 1);
                    TextFlipView.SetValue(Grid.RowProperty, 0);

                    ImageBox.SetValue(Grid.ColumnProperty, 0);
                    ImageBox.SetValue(Grid.RowProperty, 0);
                    ImageBox.Width = Column1.ActualWidth;

                    this.orientation = newOrientation;
                    this.applicationWidth = newWidth;
                }
                else if (newOrientation == ApplicationViewOrientation.Portrait || (newOrientation != ApplicationViewOrientation.Landscape && newWidth < 700))
                {
                    Column1.Width = new GridLength(Window.Current.Bounds.Width);
                    Column2.Width = new GridLength(0);
                    Row1.Height = new GridLength(Window.Current.Bounds.Height / 3);
                    Row2.Height = new GridLength(Window.Current.Bounds.Height / 3);

                    TextFlipView.SetValue(Grid.ColumnProperty, 0);
                    TextFlipView.SetValue(Grid.RowProperty, 1);

                    ImageBox.SetValue(Grid.ColumnProperty, 0);
                    ImageBox.SetValue(Grid.RowProperty, 0);
                    ImageBox.Width = Window.Current.Bounds.Width;

                    this.orientation = newOrientation;
                    this.applicationWidth = newWidth;
                }
            }
        }
    }
}
