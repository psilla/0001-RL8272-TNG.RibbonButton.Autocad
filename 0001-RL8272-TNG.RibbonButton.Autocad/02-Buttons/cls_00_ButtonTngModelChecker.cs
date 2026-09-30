using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using TYPSA.SharedLib.Autocad;
using TYPSA.SharedLib.UserForms;

namespace TNG.RibbonButton.Autocad
{
    public class UiTexts
    {
        public string Title { get; set; }
        public string MsgNoOptions { get; set; }
        public string MsgCompleted { get; set; }
        public string MsgTitle { get; set; }
    }

    public class cls_00_ButtonTngModelChecker
    {
        private static UiTexts GetUiTexts(bool isSpanish)
        {
            return new UiTexts
            {
                Title = isSpanish
                    ? "Seleccione los análisis a ejecutar:"
                    : "Select the analysis to be performed:",

                MsgNoOptions = isSpanish
                    ? "No se seleccionó ninguna opción. El proceso ha sido cancelado."
                    : "No options were selected. The process has been cancelled.",

                MsgCompleted = isSpanish
                    ? $"{nameof(RibbonCommands.ProcessTGNChecker)} finalizado correctamente."
                    : $"{nameof(RibbonCommands.ProcessTGNChecker)} completed successfully.",

                MsgTitle = isSpanish
                    ? "Proceso completado"
                    : "Process Complete"
            };
        }

        [CommandMethod(RibbonCommands.ButtonTGNChecker)]
        public static void ButtonTngModelChecker()
        {
            // try
            try
            {
                // ---------------------------------
                // Obtener datos de usuario
                // ---------------------------------

                bool userData = cls_00_GetUserData.GetUserData(
                    out string projectCode, out List<string> selectedFiles, out string selectedFolderPath,
                    out DateTime startTime,
                    customPathLabel: "Please, paste the folder containing the DWG files to analyze",
                    requestProjectCode: false
                );
                // Validamos
                if (!userData) return;

                // ---------------------------------
                // Definir código de proyecto
                // ---------------------------------

                projectCode = "RL8272";

                // ---------------------------------
                // Obtener informacion
                // ---------------------------------

                CadSessionInfo infoCad = new CadSessionInfo();

                // ---------------------------------
                // Detectar Idioma 
                // ---------------------------------

                // Detectamos
                bool isSpanish = infoCad.CivilLanguage?.Equals(
                    "Spanish", StringComparison.OrdinalIgnoreCase
                ) == true;

                // ---------------------------------
                // Texto UI segun idioma
                // ---------------------------------

                UiTexts uiTexts = GetUiTexts(isSpanish);

                // ---------------------------------
                // Form Opciones
                // ---------------------------------

                List<string> selectedOptions = cls_00_InstaForm_CheckedListBox.CheckListBoxFormSearchOut(
                    uiTexts.Title, ModelCheckerKeys.GetAllOptionsTng(isSpanish),
                    ModelCheckerKeys.GetDefaultSelectedOptionsTng(isSpanish)
                );
                // Validamos
                if (selectedOptions == null || selectedOptions.Count == 0)
                {
                    // Mensaje
                    MessageBox.Show(
                        uiTexts.MsgNoOptions, "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information
                    );
                    // Finalizamos
                    return;
                }

                // -------------------------------
                // Ejecutamos
                // -------------------------------

                cls_00_MainTngModelChecker mainProcess = new cls_00_MainTngModelChecker();
                // Procesar archivos
                ProcessResult processResult = mainProcess.MainTngModelChecker(
                    selectedFolderPath, selectedFiles.ToArray(), projectCode, 
                    selectedOptions, startTime, infoCad, uiTexts, isSpanish
                );
            }
            // catch
            catch (System.Exception ex)
            {
                // Mensaje
                MessageBox.Show(
                    "An unexpected error occurred while executing Atenea Model Checker.\n\n" +
                    ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error
                );
            }
        }


    }
}
