using UIForia.ListTypes;
using UIForia.Util.Unsafe;

namespace UIForia.Text {
    // CreateShapeKey(element.styles), pushFont() -> GetOrCreateBucket() -> PushFontSize() -> GetOrCreate() -> 
    // when a shapkey field changes -> GetOrCreateBucket(). Set that as the 'active' bucket

    // (shapeKey + wordContent) -> ShapeResultId 
    // Table<ShapeResultId, ShapeResult> 

    // for each word in text
    // assign word to a bucket 
    // after processing
    // deduplicate words 

    internal struct ShapeBucket {

        public ShapeKey key;
        public List_Char wordBuffer;
        public DataList<WordIndex> words; // other meta data to map back to origin text 

    }
}