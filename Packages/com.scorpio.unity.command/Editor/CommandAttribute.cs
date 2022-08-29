using System;
namespace Scorpio.Unity.Command {
    public class CommandAttribute : Attribute {
        public string name;
        public CommandAttribute(string name) {
            this.name = name;
        }
    }
    public class CommandFinallyAttribute : Attribute {

    }
}
