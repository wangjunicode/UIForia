using System.Text;
using UnityEngine;

namespace UIForia.Util {

    public class IndentedStringBuilder {

        private int indentLevel;
        private readonly StringBuilder builder;

        public IndentedStringBuilder(uint capacity) {
            this.builder = new StringBuilder((int)capacity);
        }

        public void Indent(int level = 1) {
            indentLevel += level;
        }

        public void Outdent(int level = 1) {
            indentLevel -= level;
            if (indentLevel < 0) indentLevel = 0;
        }

        public void Append(string str) {
            if (indentLevel != 0) {
                for (int i = 0; i < indentLevel * 4; i++) {
                    builder.Append(' ');
                }
            }

            builder.Append(str);
        }

        public void Clear() {
            builder.Clear();
            indentLevel = 0;
        }
        
        public override string ToString() {
            return builder.ToString();
        }

        public bool enableColors;
        
        public void PushColor(Color color) {
            if (enableColors) {
                AppendInline($"<color=#{(byte) (color.r * 255f):X2}{(byte) (color.g * 255f):X2}{(byte) (color.b * 255f):X2}>");
            }    
        }

        public void PopColor() {
            if (enableColors) {
                AppendInline("</color>");
            }   
        }

        public void AppendInline(string str) {
            builder.Append(str);
        }
        
        
        public void AppendInline(Color color, string str) {
            if (enableColors) {
              PushColor(color);
              builder.Append(str);
              PopColor();
            }
            else {
                builder.Append(str);
            }
        }
        
        public void NewLine() {
            builder.Append('\n');
        }

    }

}