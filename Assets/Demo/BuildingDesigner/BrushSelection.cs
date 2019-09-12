namespace Demo {

    public enum Category {
        Basic,
        Block,
        Template
    }

    public enum BasicCategory {
        PERP,
        BEVEL,
        ROUND
    }

    public enum TemplateCategory {
        SEED,
        PLAYER
    }

    public struct BrushSelection {

        public Category category;
        public BasicCategory basic;
        public TemplateCategory templates;
        public int id;
    }
}
