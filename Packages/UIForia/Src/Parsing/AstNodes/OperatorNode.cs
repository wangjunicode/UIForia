using System;

namespace UIForia.Parsing {

    public class OperatorNode : ASTNode {

        public ASTNode left;
        public ASTNode right;
        public OperatorType operatorType;

        public int priority {
            get {
                switch (operatorType) {
                    case OperatorType.Plus:
                        return 1;
                    
                    case OperatorType.Minus:
                        return 1;
                    
                    case OperatorType.Mod:
                        return 1;
                    
                    case OperatorType.Times:
                        return 2;
                    
                    case OperatorType.Divide:
                        return 2;
                    
                    case OperatorType.TernaryCondition:
                        return -2;
                    
                    case OperatorType.TernarySelection:
                        return -1;
                    
                    case OperatorType.Equals:
                        return -1;
                    
                    case OperatorType.NotEquals:
                        return -1;
                    
                    case OperatorType.GreaterThan:
                        return -1;
                    
                    case OperatorType.GreaterThanEqualTo:
                        return -1;
                    
                    case OperatorType.LessThan:
                        return -1;
                    
                    case OperatorType.LessThanEqualTo:
                        return -1;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override void Release() {
            left.Release();
            right.Release();
            s_OperatorPool.Release(this);
        }

    }

}