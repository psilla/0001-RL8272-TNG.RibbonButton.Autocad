using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using TYPSA.SharedLib.Autocad.GetDocument;
using TYPSA.SharedLib.Autocad.Main;
using TYPSA.SharedLib.EndPoints;
using TYPSA.SharedLib.UserForms;
using TYPSA.SharedLib.Json;
using static TYPSA.SharedLib.Autocad.Main.cls_00_CadInfoHelper;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using TYPSA.SharedLib.ExcelAutocad;
using TYPSA.SharedLib.Metrics;

namespace TYPSA.PS.RibbonButton.Autocad
{
    public static class TngModelCheckerDefaults
    {
        // -----------------------------
        // Fonts
        // -----------------------------

        public const string ExpectedPaperTextFont = "ISOCPEUR";

        // -----------------------------
        // Block Names
        // -----------------------------

        public const string PlotTagBlockName = "FUT_Ritningsram_";
        public const string BlockAttributesName = "FUT_Namnruta_";

        // -----------------------------
        // Plot Tag Reference Texts
        // -----------------------------

        public const string PlotFileText = "Ritningsfil:";
        public const string PlotDateText = "Plottdatum:";
        public const string PlotUserText = "Plottad av:";

        public static readonly List<string> PlotTagReferenceTexts =
            new List<string> {PlotFileText, PlotDateText, PlotUserText};
    }

    public class cls_00_MainTngModelChecker
    {
        private static void SkipNullFile(
            bool isSpanish,
            string fileName,
            ref int processedFiles,
            int totalFiles,
            int percentage,
            ProgressBarControl progressBarForm,
            int durationMs = 1000
        )
        {
            // Mostramos
            new AutoCloseMessageForm(
                isSpanish
                    ? $"El documento '{fileName}' no se pudo abrir."
                    : $"The document '{fileName}' could not be opened.",
                durationMs
            ).ShowDialog();
            // Actualizar progreso
            processedFiles++;
            percentage = (int)((double)processedFiles / totalFiles * 100);
            progressBarForm.ProgressValue = percentage;
        }

        public ProcessResult MainTngModelChecker(
            string selectedFolderPath,
            string[] selectedFiles,
            string projectCode,
            List<string> selectedOptions,
            DateTime startTime,
            CadSessionInfo info,
            UiTexts uiTexts,
            bool isSpanish
            
        )
        {
            // -------------------------------
            // Definir variables
            // -------------------------------

            List<Dictionary<string, object>> dataJsonByModel = new List<Dictionary<string, object>>();
            List<WarningCheckLogResult> warningChecksLog = new List<WarningCheckLogResult>();
            List<List<string>> dataToExcelGlobal = new List<List<string>>();
            ModelCheckerResults resultsfromcad = new ModelCheckerResults();
            ModelCheckerKeys keys = new ModelCheckerKeys(isSpanish);
            string msg = string.Empty;

            // -------------------------------
            // Tiempos de ejecución
            // -------------------------------

            // Global
            Dictionary<string, TimeSpan> processDurations = new Dictionary<string, TimeSpan>();
            // Modelo
            Dictionary<string, Dictionary<string, TimeSpan>> processDurationsByModel =
                new Dictionary<string, Dictionary<string, TimeSpan>>();

            Stopwatch totalStopwatch = Stopwatch.StartNew();

            // -------------------------------
            // Normalizar idioma
            // -------------------------------

            string softwareLanguage = isSpanish ? "Spanish" : "English";

            // -------------------------------
            // Obtener informacion
            // -------------------------------

            string keyElementData = cls_00_AteneaJson.CivilElementData;
            string keyFileName = cls_00_AteneaJson.FileName;

            // -------------------------------
            // Crear progreso
            // -------------------------------

            ProgressBarControl progressBarForm = new ProgressBarControl();
            // Iniciamos variables
            int totalFiles = selectedFiles.Length;
            int processedFiles = 0;
            int percentage = 0;

            // Mostramos
            progressBarForm.ProgressValue = 10; // Valor inicial 
            progressBarForm.Show();

            // try
            try
            {
                // -----------------------------
                // Iniciar cronometro modelos
                // -----------------------------

                string msgIter = cls_00_ProcessMessages.ShowProcessMessage(
                    isSpanish, cls_00_ProcessMessages.StartDocumentProcessing
                );

                Stopwatch filesProcessingStopwatch = Stopwatch.StartNew();

                // -----------------------------
                // Iterar archivos
                // -----------------------------

                foreach (string file in selectedFiles)
                {
                    // -------------------------------
                    // Definir variables
                    // -------------------------------

                    List<Dictionary<string, object>> extractedData = new List<Dictionary<string, object>>();

                    string fileName = System.IO.Path.GetFileNameWithoutExtension(file);

                    Dictionary<string, TimeSpan> modelDurations = new Dictionary<string, TimeSpan>();

                    // try
                    try
                    {
                        // -------------------------------
                        // Abrir documento
                        // -------------------------------

                        msg = cls_00_ProcessMessages.ShowProcessMessage(
                            isSpanish, cls_00_ProcessMessages.OpenDocument, processedFiles + 1, totalFiles, fileName
                        );

                        Stopwatch modelProcessStopwatch = Stopwatch.StartNew();
                      
                        using (Document openedDoc = Application.DocumentManager.Open(file, false))
                        {
                            cls_00_ProcessMessages.AddProcessDuration(
                                modelDurations, msg, modelProcessStopwatch
                            );

                            // -------------------------------
                            // Validar documento
                            // -------------------------------

                            if (openedDoc == null)
                            {
                                // Obviamos
                                SkipNullFile(
                                    isSpanish, fileName, ref processedFiles, totalFiles, percentage, progressBarForm
                                );
                                continue;
                            }

                            // -------------------------------
                            // Procesar documento
                            // -------------------------------

                            msg = cls_00_ProcessMessages.ShowProcessMessage(
                                isSpanish, cls_00_ProcessMessages.AnalyzeDocument, processedFiles + 1, totalFiles, fileName
                            );

                            // -------------------------------
                            // Obtener info
                            // -------------------------------

                            Database db = openedDoc.Database;
                            Editor ed = cls_00_DocumentInfo.GetEditor(openedDoc);

                            // -------------------------------
                            // Bloquear el documento
                            // -------------------------------

                            using (openedDoc.LockDocument())
                            using (Transaction tr = openedDoc.TransactionManager.StartTransaction())
                            {
                                // try
                                try
                                {
                                    // Obtener BlockTable
                                    BlockTable bt = cls_00_DocumentInfo.GetBlockTableForRead(tr, db);

                                    // -------------------------------
                                    // Procesar info
                                    // -------------------------------

                                    msg = cls_00_ProcessMessages.ShowProcessMessage(
                                        isSpanish, cls_00_ProcessMessages.CollectModelData, fileName
                                    );

                                    modelProcessStopwatch = Stopwatch.StartNew();

                                    cls_00_ProcessModelChecks.ProcessModelChecks(
                                        selectedOptions, keys, tr, db, bt, fileName, warningChecksLog,
                                        resultsfromcad, extractedData, isSpanish
                                    );

                                    cls_00_ProcessMessages.AddProcessDuration(modelDurations, msg, modelProcessStopwatch);

                                    // -------------------------------
                                    // Construir y almacenar informacion
                                    // -------------------------------

                                    msg = cls_00_ProcessMessages.ShowProcessMessage(
                                        isSpanish, cls_00_ProcessMessages.TransformModelData, fileName
                                    );

                                    modelProcessStopwatch = Stopwatch.StartNew();

                                    Dictionary<string, object> fileDataByDoc = new Dictionary<string, object>
                                    {
                                        { keyFileName, fileName },
                                        { keyElementData, extractedData }
                                    };

                                    // Añadimos
                                    dataJsonByModel.Add(fileDataByDoc);

                                    cls_00_ProcessMessages.AddProcessDuration(modelDurations, msg, modelProcessStopwatch);

                                    // -----------------------------
                                    // Cerrar transaccion
                                    // -----------------------------

                                    tr.Commit();
                                }
                                // catch
                                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                                {
                                    warningChecksLog.Add(new WarningCheckLogResult
                                    {
                                        FileName = fileName,
                                        CheckName = "Processing Error",
                                        Message = ex.Message
                                    });

                                    // Mensaje
                                    new AutoCloseMessageForm(
                                        $"Error while processing '{file}':\n{ex.Message}"
                                    ).ShowDialog();
                                }
                            }

                            // -----------------------------
                            // Cerrar documento
                            // -----------------------------

                            msg = cls_00_ProcessMessages.ShowProcessMessage(
                                isSpanish, cls_00_ProcessMessages.CloseDocument, processedFiles + 1, totalFiles, fileName
                            );

                            Stopwatch closeDocumentStopwatch = Stopwatch.StartNew();

                            openedDoc.CloseAndDiscard();

                            cls_00_ProcessMessages.AddProcessDuration(modelDurations, msg, closeDocumentStopwatch);

                            // -------------------------------
                            // Tiempo total de procesos registrados
                            // -------------------------------

                            if (modelDurations.Any())
                            {
                                TimeSpan modelTotalDuration = TimeSpan.FromTicks(
                                    modelDurations.Values.Sum(x => x.Ticks)
                                );

                                modelDurations["Total"] = modelTotalDuration;

                                // Guardamos los tiempos del modelo
                                processDurationsByModel[fileName] = modelDurations;
                            }

                            // -----------------------------
                            // Actualizar progreso
                            // -----------------------------

                            processedFiles++;
                            percentage = (int)((double)processedFiles / totalFiles * 100);
                            progressBarForm.ProgressValue = percentage;
                        }
                    }
                    // catch
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        warningChecksLog.Add(new WarningCheckLogResult
                        {
                            FileName = fileName,
                            CheckName = "Fatal File Error",
                            Message = ex.Message
                        });

                        // Mensaje
                        MessageBox.Show(
                            $"EXCEPTION:\n{ex.Message}\n{ex.StackTrace}"
                        );
                    }
                }

                // Añadimos
                cls_00_ProcessMessages.AddProcessDuration(processDurations, msgIter, filesProcessingStopwatch);

                // -----------------------------
                // Cerrar progreso
                // -----------------------------

                progressBarForm.Close();

                // -----------------------------
                // Validar info
                // -----------------------------

                msg = isSpanish
                    ? "Validando la información recopilada de todos los documentos"
                    : "Validating the information collected from all documents";
                // Mensaje
                new AutoCloseMessageForm(msg, 1000).ShowDialog();

                bool hasData = 
                    resultsfromcad.ProjectUnits.Any() || resultsfromcad.LayersInUse.Any() || resultsfromcad.LayerZero.Any() || 
                    resultsfromcad.Version.Any() || resultsfromcad.Xrefs.Any() || resultsfromcad.PaperTextFont.Any() || resultsfromcad.ByLayer.Any() || 
                    resultsfromcad.RevisionClouds.Any() || resultsfromcad.BlockAttributes.Any() || resultsfromcad.PlotTags.Any();
                // Validamos
                if (hasData || warningChecksLog.Any())
                {
                    // ---------------------------------
                    // Preparar datos exportacion
                    // ---------------------------------

                    Dictionary<string, object> exportData = cls_00_GetExportData.GetAllExportData(
                        keys, selectedOptions, resultsfromcad
                    );

                    // ---------------------------------
                    // Validar
                    // ---------------------------------

                    if (warningChecksLog.Any())
                    {
                        exportData.Add("Warning Selected Checks Log", warningChecksLog);
                    }

                    // ---------------------------------
                    // Exportar Report
                    // ---------------------------------

                    msg = isSpanish
                        ? "Generando los informes finales en Excel y HTML"
                        : "Generating the final Excel and HTML reports";
                    // Mensaje
                    new AutoCloseMessageForm(msg, 1000).ShowDialog();

                    // Excel
                    cls_00_ExportModCheckToExcel_OpenXml.ExportDataToExcel(exportData);

                    // Html
                    cls_00_ExportTgnCheckToHtml.ExportToHtml(
                        selectedFolderPath, exportData, warningChecksLog, projectCode, totalFiles, processedFiles
                    );
                }
                else
                {
                    // Mensaje
                    MessageBox.Show(
                        "No data found for the selected checks.", "Atenea Model Checker",
                        MessageBoxButtons.OK, MessageBoxIcon.Information
                    );
                }

                // -----------------------------
                // Exportar JSON Global
                // -----------------------------

                // try
                try
                {
                    // Preparar diccionario a enviar
                    Dictionary<string, object> dictDataByFileToJson = GetFinalJsonDictionaryTng(
                        projectCode, softwareLanguage, dataJsonByModel
                    );
                    // Exportamos
                    cls_00_SaveJson.TrySaveJson(
                        isSpanish, dictDataByFileToJson, projectCode, info.RootFolderNameTng, 
                        info.JsonFileNameDataExtractionTng, selectedFolderPath
                    );
                }
                // catch
                catch (Exception ex)
                {
                    string inner = ex.InnerException != null
                        ? $"\n\nInner:\n{ex.InnerException.Message}"
                        : "";
                    // Mostramos
                    MessageBox.Show(
                        "Error generating JSON\n\n" + ex.Message + inner + "\n\nStack:\n" + ex.StackTrace,
                        "JSON Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                }

                // Enviar Metricas Serapis
                cls_00_SerapisMetrics.InitializeMetricsAsync("6a3541d327f9ca81c8d1fa0d");

                // -------------------------------
                // Tiempo total
                // -------------------------------

                totalStopwatch.Stop();

                processDurations["Total"] = totalStopwatch.Elapsed;

                // -------------------------------
                // Resumen
                // -------------------------------

                DateTime endTime = DateTime.Now;
                TimeSpan duration = endTime - startTime;

                // Mensaje
                MessageBox.Show(
                    uiTexts.MsgCompleted +
                    "\nDuration: " + duration.ToString(@"hh\:mm\:ss") +
                    "\nStarted at: " + startTime.ToString("HH:mm:ss") +
                    "\nEnded at: " + endTime.ToString("HH:mm:ss"),
                    uiTexts.Title, MessageBoxButtons.OK, MessageBoxIcon.Information
                );

                // return
                return new ProcessResult
                {
                    TotalFilesProcessed = processedFiles,
                    ParametersAnalyzed = processedFiles
                };
            }
            // catch
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // Por defecto
            return new ProcessResult();
        }
        

    }
}
