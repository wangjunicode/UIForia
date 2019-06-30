using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    public class AnimationRootNode : StyleASTNode {

        public string animName;

        public LightList<VariableDefinitionNode> variableNodes;
        public LightList<AnimationOptionNode> optionNodes;
        public LightList<KeyFrameNode> keyFrameNodes;
            
        public AnimationRootNode(string animName) {
            this.animName = animName;
            type = StyleASTNodeType.AnimationDeclaration;
        }

        public void AddVariableNode(VariableDefinitionNode node) {
            variableNodes = variableNodes ?? new LightList<VariableDefinitionNode>(4);
            variableNodes.Add(node);
        }
        
        public void AddOptionNode(AnimationOptionNode optionNode) {
            optionNodes = optionNodes ?? new LightList<AnimationOptionNode>(4);
            optionNodes.Add(optionNode);
        }

        public void AddKeyFrameNode(KeyFrameNode keyFrameNode) {
            keyFrameNodes = keyFrameNodes ?? new LightList<KeyFrameNode>(4);
            keyFrameNodes.Add(keyFrameNode);
        }

        public override void Release() {
            if (variableNodes != null) {
                for (int i = 0; i < variableNodes.Count; i++) {
                    variableNodes[i].Release();
                }
                variableNodes.Clear();
                LightList<VariableDefinitionNode>.Release(ref variableNodes);
            }
            
            if (optionNodes != null) {
                for (int i = 0; i < optionNodes.Count; i++) {
                    optionNodes[i].Release();
                }
                optionNodes.Clear();
                LightList<AnimationOptionNode>.Release(ref optionNodes);
            }
            
            if (keyFrameNodes != null) {
                for (int i = 0; i < keyFrameNodes.Count; i++) {
                    keyFrameNodes[i].Release();
                }
                keyFrameNodes.Clear();
                LightList<KeyFrameNode>.Release(ref keyFrameNodes);
            }
        }

    }

}