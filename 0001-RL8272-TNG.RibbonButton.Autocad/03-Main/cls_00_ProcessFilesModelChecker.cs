using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad;
using TYPSA.SharedLib.EndPoints;
using TYPSA.SharedLib.UserForms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TNG.RibbonButton.Autocad
{
    public class cls_00_ProcessFilesModelChecker
    {
        private static void SkipNullFile(
            bool isSpanish,
            string fileName,
            ref int processedFiles,
            int totalFiles,
            ProgressBarControl progressBarForm,
            int durationMs = 1000
        )
        {
            // -----------------------------
            // Mostrar mensaje
            // -----------------------------

            new AutoCloseMessageForm(
                isSpanish
                    ? $"El documento '{fileName}' no se pudo abrir."
                    : $"The document '{fileName}' could not be opened.",
                durationMs
            ).ShowDialog();

            // -----------------------------
            // Actualizar progreso
            // -----------------------------

            processedFiles++;
            int percentage = (int)((double)processedFiles / totalFiles * 100);
            progressBarForm.ProgressValue = percentage;
        }

        public static void ProcessSelectedModels(
            string[] selectedFiles,
            List<string> selectedOptions,
            ModelCheckerKeys keys,
            bool isSpanish,
            string keyFileName,
            string keyElementData,
            List<WarningCheckLogResult> warningChecksLog,
            ModelCheckerResults resultsfromcad,
            List<Dictionary<string, object>> dataJsonByModel,
            Dictionary<string, TimeSpan> processDurations,
            Dictionary<string, Dictionary<string, TimeSpan>> processDurationsByModel,
            ProgressBarControl progressBarForm,
            ref int processedFiles
        )
        {
            // -----------------------------
            // Definir variables
            // -----------------------------

            int totalFiles = selectedFiles.Length;

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

                    string msg = cls_00_ProcessMessages.ShowProcessMessage(
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
                                isSpanish, fileName, ref processedFiles, totalFiles, progressBarForm
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
                        // Obtener informacion
                        // -------------------------------

                        Database db = openedDoc.Database;

                        // -------------------------------
                        // Bloquear documento
                        // -------------------------------

                        using (openedDoc.LockDocument())
                        using (Transaction tr = openedDoc.TransactionManager.StartTransaction())
                        {
                            // try
                            try
                            {
                                // -------------------------------
                                // Obtener BlockTable
                                // -------------------------------

                                BlockTable bt = cls_00_DocumentInfo.GetBlockTableForRead(tr, db);

                                // -------------------------------
                                // Procesar informacion
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

                                cls_00_ProcessMessages.AddProcessDuration(
                                    modelDurations, msg, modelProcessStopwatch
                                );

                                // -----------------------------
                                // Cerrar transaccion
                                // -----------------------------

                                tr.Commit();
                            }
                            // catch
                            catch (Autodesk.AutoCAD.Runtime.Exception ex)
                            {
                                warningChecksLog.Add(
                                    new WarningCheckLogResult
                                    {
                                        FileName = fileName,
                                        CheckName = "Processing Error",
                                        Message = ex.Message
                                    }
                                );

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
                        int percentage = (int)((double)processedFiles / totalFiles * 100);
                        progressBarForm.ProgressValue = percentage;
                    }
                }
                // catch
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    warningChecksLog.Add(
                        new WarningCheckLogResult
                        {
                            FileName = fileName,
                            CheckName = "Fatal File Error",
                            Message = ex.Message
                        }
                    );

                    // Mensaje
                    MessageBox.Show(
                        $"EXCEPTION:\n{ex.Message}\n{ex.StackTrace}"
                    );
                }
            }

            // -----------------------------
            // Registrar tiempo total
            // -----------------------------

            cls_00_ProcessMessages.AddProcessDuration(
                processDurations, msgIter, filesProcessingStopwatch
            );
        }
    }


}