using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.AutoCAD.Runtime;
using _0001_RL8272_TNG.RibbonButton.Autocad.Properties;

namespace TYPSA.PS.RibbonButton.Autocad
{
    public static class RibbonCommands
    {
        public const string ButtonTGNModelChecker = "ButtonTGNModelChecker";
    }

    public class App : IExtensionApplication
    {
        public void Initialize()
        {

            LoadRibbon();
        }

        public void Terminate()
        {
            // Aquí puedes realizar acciones cuando la aplicación se descarga.
        }

        private void LoadRibbon()
        {
            Autodesk.Windows.RibbonControl ribbonControl = Autodesk.Windows.ComponentManager.Ribbon;
            if (ribbonControl != null)
            {
                /////////////// CREAR RIBBON //////////////////////

                Autodesk.Windows.RibbonTab rtab = new Autodesk.Windows.RibbonTab();
                rtab.Title = "TYPSA-TNG";
                rtab.Id = "TESTRIBBON_TAB_ID";
                ribbonControl.Tabs.Add(rtab);

                /////////////// CREAR PANELES //////////////////////

                // Panel1
                Autodesk.Windows.RibbonPanelSource rps1 = new Autodesk.Windows.RibbonPanelSource();
                rps1.Title = "TNG DWG DRAWING CHECKER";
                Autodesk.Windows.RibbonPanel rp5 = new Autodesk.Windows.RibbonPanel();
                rp5.Source = rps1;
                rtab.Panels.Add(rp5);

                /////////////// ADDING BUTTONS //////////////////////

                // TNG MC
                Autodesk.Windows.RibbonButton buttonAteneaMC = CreateRibbonButton(
                    name: "TNG DWG Drawing Checker",
                    text: "TNG DWG Drawing Checker",
                    image: Resources.AteneaModelCheckerCivil,
                    commandParameter: RibbonCommands.ButtonTGNModelChecker,
                    tooltipTitle: "",
                    tooltipContent: ""
                );
                // Añadimos button
                rps1.Items.Add(buttonAteneaMC);

                /////////////// ACTIVAR RIBBON //////////////////////

                rtab.IsActive = true;
            }
        }

        // Define a command handler class implementing the ICommand interface for ribbon button actions.
        public class MyRibbonCommandHandler : System.Windows.Input.ICommand
        {
            // Determines whether the command can be executed. Always returns true in this case.
            public bool CanExecute(object parameter)
            {
                return true;
            }

            // Event that must be declared when implementing ICommand, 
            // but it's not used here. It's for handling changes in command execution state.
            public event EventHandler CanExecuteChanged;

            // Executes the actual command logic based on the parameter passed, typically a ribbon button.
            public void Execute(object parameter)
            {
                // Check if the parameter is a RibbonButton from Autodesk's UI components.
                if (parameter is Autodesk.Windows.RibbonButton ribbonButton)
                {
                    // Retrieve the command parameter from the ribbon button, expected to be a string.
                    string command = ribbonButton.CommandParameter as string;
                    switch (command)
                    {
                        case RibbonCommands.ButtonTGNModelChecker:
                            // Instanciamos la clase
                            cls_00_ButtonTngModelChecker.ButtonAteneaModelChecker();
                            break;

                        // Default case for unhandled commands. No action is taken.
                        default:
                            break;
                    }
                }
            }
        }

        private BitmapImage GetImageSource(Image img)
        {
            try
            {
                if (img == null)
                {
                    throw new ArgumentNullException(nameof(img), "La imagen no puede ser null.");
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    Bitmap bitmap = new Bitmap(img);
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;

                    BitmapImage bmpImg = new BitmapImage();
                    bmpImg.BeginInit();
                    bmpImg.CacheOption = BitmapCacheOption.OnLoad;
                    bmpImg.StreamSource = ms;
                    bmpImg.EndInit();
                    bmpImg.Freeze(); // Evita problemas de hilos en WPF y AutoCAD

                    return bmpImg;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"❌ ERROR en GetImageSource: {ex.Message}",
                    "Error de Conversión de Imagen"
                );
                return null;
            }
        }

        private Autodesk.Windows.RibbonButton CreateRibbonButton(
            string name,
            string text,
            Image image,
            string commandParameter,
            string tooltipTitle,
            string tooltipContent
        )
        {
            ImageSource imageSource = GetImageSource(image);

            Autodesk.Windows.RibbonButton button = new Autodesk.Windows.RibbonButton
            {
                Name = name,
                ShowText = true,
                Text = text,
                ShowImage = true,
                LargeImage = imageSource,
                Size = Autodesk.Windows.RibbonItemSize.Large,
                CommandHandler = new MyRibbonCommandHandler(),
                CommandParameter = commandParameter,
                // Tooltip
                ToolTip = new Autodesk.Windows.RibbonToolTip
                {
                    Title = tooltipTitle,
                    Content = tooltipContent,
                    IsHelpEnabled = false
                }
            };

            return button;
        }

       





    }
}