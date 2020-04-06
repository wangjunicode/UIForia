using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace UIForia.Editor {

    [InitializeOnLoad]
    public class CompileErrorChecker {

        static CompileErrorChecker() {
            CompilationPipeline.assemblyCompilationFinished += OnCompileFinished;
        }

        private static readonly HashSet<string> s_Files = new HashSet<string>();
        
        private static void OnCompileFinished(string s, CompilerMessage[] compilerMessages) {

            s_Files.Clear();
            
            for (int i = 0; i < compilerMessages.Length; i++) {
                
                if (compilerMessages[i].type == CompilerMessageType.Error && compilerMessages[i].file.Contains("UIForiaGenerated")) {
                    s_Files.Add(compilerMessages[i].file);
                }
                
            }

            foreach (string file in s_Files) {
                Debug.Log("Detected compile error in UIForia generated file. Deleting " + file);
                // todo -- actually delete the file
            }
            
        }
        
    }

}