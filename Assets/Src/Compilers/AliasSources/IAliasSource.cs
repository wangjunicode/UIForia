namespace Src.Compilers.AliasSource {

    public interface IAliasSource {

        object ResolveAlias(string alias, object data = null);

    }

}