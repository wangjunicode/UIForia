using System;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public class Diagnostics {

        public LightList<DiagnosticEntry> diagnosticList;

        private DiagnosticType type;

        public Diagnostics() {
            this.diagnosticList = new LightList<DiagnosticEntry>();
        }

        public void LogInfo(string message) {
            diagnosticList.Add(new DiagnosticEntry() {
                message = message,
                diagnosticType = DiagnosticType.Info,
                timestamp = DateTime.Now
            });
        }
        
        public void LogWarning(string message) {
            diagnosticList.Add(new DiagnosticEntry() {
                message = message,
                diagnosticType = DiagnosticType.Warning,
                timestamp = DateTime.Now
            });
        }

        public void LogError(string message) {
            diagnosticList.Add(new DiagnosticEntry() {
                message = message,
                diagnosticType = DiagnosticType.Error,
                timestamp = DateTime.Now
            });
        }

        public void LogError(string message, string filePath, int lineNumber = 0, int columnNumber = 0) {
            diagnosticList.Add(new DiagnosticEntry() {
                message = message,
                filePath = filePath,
                lineNumber = lineNumber,
                columnNumber = columnNumber,
                diagnosticType = DiagnosticType.Error,
                timestamp = DateTime.Now
            });
        }
        
        public void LogException(Exception exception) {
            diagnosticList.Add(new DiagnosticEntry() {
                message = exception.Message,
                exception = exception,
                diagnosticType = DiagnosticType.Exception,
                timestamp = DateTime.Now
            });
        }
        
        public void Clear() {
            diagnosticList.Clear();
        }

        public bool HasErrors() {
            for (int i = 0; i < diagnosticList.size; i++) {
                if ((diagnosticList.array[i].diagnosticType == DiagnosticType.Error) || diagnosticList.array[i].diagnosticType == DiagnosticType.Exception) {
                    return true;
                }
            }

            return false;
        }

        public void Dump() {
            for (int i = 0; i < diagnosticList.size; i++) {
                ref DiagnosticEntry diag = ref diagnosticList.array[i];
                string retn = "";
                switch (diag.diagnosticType) {

                    case DiagnosticType.Info:
                        retn += "[UIForia::Info] ";
                        break;

                    case DiagnosticType.Error:
                        retn += "[UIForia::Error] ";
                        break;

                    case DiagnosticType.Exception:
                        retn += "[UIForia::Exception] ";
                        break;

                    case DiagnosticType.Warning:
                        retn += "[UIForia::Warning] ";
                        break;

                    default:
                        retn += "[UIForia::Info] ";
                        break;
                }

                if (diag.filePath != null) {
                    int idx = diag.filePath.IndexOf("Assets");
                    if (idx != -1) {
                        retn += "(at " + diag.filePath.Substring(idx);
                    }
                    else {
                        retn += "(at " + diag.filePath;
                    }

                    if (diag.lineNumber >= 0 && diag.columnNumber < 0) {
                        retn += ":" + diag.lineNumber + ") ";
                    }
                    else if (diag.lineNumber >= 0 && diag.columnNumber >= 0) {
                        retn += ":" + diag.lineNumber + "," + diag.columnNumber + ") ";
                    }
                    else {
                        retn += ") ";
                    }
                }

                retn += diag.message;

                switch (diag.diagnosticType) {
                    case DiagnosticType.Info:
                        Debug.Log(retn);
                        break;

                    case DiagnosticType.Error:
                    case DiagnosticType.Exception:
                        Debug.LogError(retn);
                        break;

                    case DiagnosticType.Warning:
                        Debug.LogWarning(retn);
                        break;

                    default:
                        Debug.Log(retn);
                        break;
                }

            }
        }

        

    }

}