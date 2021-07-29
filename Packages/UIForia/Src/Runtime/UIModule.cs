using System;
using System.Collections.Generic;
using System.IO;
using UIForia.Compilers;
using UIForia.Style;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    [CreateAssetMenu(fileName = "NewModule", menuName = "UIForia/Create UI Module")]
    public class UIModule : ScriptableObject {

        public UIModule[] referencedModules;

        internal string location;
        internal string path;
        internal string moduleName;
        
        internal void OnEnable() {
            moduleName = name;
        }
        
        // todo -- somehow ensure no duplicate names and only 1 per hierarchy

        // todo -- move these dictionaries elsewhere 
       // internal Dictionary<string, Type> looseElements;
       // internal Dictionary<string, ResolvedTag> tagMap;
       // internal Dictionary<string, TemplateReference> templateMap;


        internal void Initialize() {
            // todo -- figure out the distinction between the compile time and run time notion of a module 
            // custom asset type? wrap in #ifdef EDITOR ? 

           // tagMap = new Dictionary<string, ResolvedTag>();
           // looseElements = new Dictionary<string, Type>();
           // templateMap = new Dictionary<string, TemplateReference>();
        }

        public string GetStyleFileByPath(string templatePath, string stylePath) {
            return Path.Combine(location, stylePath);
        }

    }

    // internal struct FunctionInfo {
    //
    //     public string name;
    //     public Argument[] arguments;
    //     public Type argumentsType;
    //     public Type contextType;
    //     public ushort firstChildIndex;
    //     public UITemplate template;
    //     public FunctionDefinitionLevel definitionLevel;
    //     public int renderCallCount;
    //     public FieldInfo renderFn;
    //     public InitialTemplateCompiler.FunctionType functionType;
    //
    // }

}