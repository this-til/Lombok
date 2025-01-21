using System.Text;

namespace Til.Lombok {

    public class ProfundityStringBuilder {

        public readonly StringBuilder stringBuilder;
        
        public int indentation = 0;
        
        public ProfundityStringBuilder() {
            stringBuilder = new StringBuilder();
        }

        public ProfundityStringBuilder(StringBuilder stringBuilder) {
            this.stringBuilder = stringBuilder;
        }

        public ProfundityStringBuilder Append(string s) {
            stringBuilder.Append(s);
            return this;
        }

        public ProfundityStringBuilder AppendLine() {
            stringBuilder.Append("\r\n");
            for (int i = 0; i < indentation; i++) {
                stringBuilder.Append("  ");
            }
            return this;
        }

        public ProfundityStringBuilder addIndentation() {
            indentation++;
            return this;
        }
        
        public ProfundityStringBuilder decreaseIndentation() {
            indentation--;
            return this;
        }
        

        public override string ToString() => stringBuilder.ToString();

    }
}
