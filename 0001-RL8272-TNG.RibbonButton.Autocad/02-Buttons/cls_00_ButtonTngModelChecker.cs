using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using TYPSA.SharedLib.Autocad.Buttons;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.Excel;
using TYPSA.SharedLib.UserForms;

namespace TYPSA.PS.RibbonButton.Autocad
{
    public class cls_00_ButtonTngModelChecker
    {
        [CommandMethod(RibbonCommands.ButtonTGNModelChecker)]
        public static void ButtonAteneaModelChecker()
        {
            // ---------------------------------
            // Obtener datos de usuario
            // ---------------------------------

            bool userData = cls_00_GetUserData.GetUserData(
                out string projectCode,
                out List<string> selectedFiles,
                out string selectedFolderPath,
                out DateTime startTime,
                "Please, paste the folder containing the DWG files to analyze"
            );
            // Validamos
            if (!userData) return;

            // ---------------------------------
            // Detectar Idioma Autocad
            // ---------------------------------

            CadSessionInfo info = new CadSessionInfo();
            // Detectamos
            bool isSpanish = info.CivilLanguage?.Equals(
                "Spanish", StringComparison.OrdinalIgnoreCase
            ) == true;

            // Texto UI según idioma
            string title = isSpanish
                ? "Seleccione los análisis a ejecutar:"
                : "Select the analysis to be performed:";

            string msgNoOptions = isSpanish
                ? "No se seleccionó ninguna opción. El proceso ha sido cancelado."
                : "No options were selected. The process has been cancelled.";

            string msgCompleted = isSpanish
                ? "Atenea Model Checker finalizado correctamente."
                : "Atenea Model Checker completed successfully.";

            string msgTitle = isSpanish
                ? "Proceso completado"
                : "Process Complete";

            // ---------------------------------
            // Form Opciones
            // ---------------------------------

            List<string> selectedOptions = cls_00_InstaForm_CheckedListBox.CheckListBoxFormSearchOut(
                title,
                AteneaModelCheckerOptionsLocalized.GetAllOptions(isSpanish),
                AteneaModelCheckerOptionsLocalized.GetDefaultSelectedOptions(isSpanish)
            );
            // Validamos
            if (selectedOptions == null || selectedOptions.Count == 0)
            {
                // Mensaje
                MessageBox.Show(
                    msgNoOptions, "Information",
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
            ProcessResult processResult = mainProcess.MainTgnModelChecker(
                selectedFiles.ToArray(), projectCode, selectedOptions, isSpanish, info
            );

            // -------------------------------
            // Summary
            // -------------------------------

            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

            // Mensaje
            MessageBox.Show(
                msgCompleted +
                "\nDuration: " + duration.ToString(@"hh\:mm\:ss") +
                "\nStarted at: " + startTime.ToString("HH:mm:ss") +
                "\nEnded at: " + endTime.ToString("HH:mm:ss"),
                msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Information
            );
        }


    }
}
