using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParseException : System.Exception { }

public abstract class AliasResolver { }

public struct Context {

    public object rootContext;
    public object currentContext;

}

public interface IExpression<out T> {

    T Evaluate(Context context);

}

//public struct Parser {
//
//    public List<DSLToken> Parse() {
//        
//    }
//
//}

public class Compiler {

    protected List<AliasResolver> m_AliasResolvers;
    
    public IExpression<T> Compile<T>(Type rootContext, string expression) {
        try {
            
        }
        catch (ParseException ex) {
            
        }
        return default;
    }
    
}

/*

Linq / Reflection / Code gen
Custom expressions ($router.parameter)

handle null
handle default

Bitwise ops => cast to true / false?
Inline Array / Dictionary
Casting
Lambdas
Operator overloads in expressions
new for structs
typeof
using
error handling / not blowing up
is 
as

struct Context {
    public object rootContext;
    public object currentContext;
}

<Using namespace="UIForia"/>
<Using value="" as=""/>

LayoutType.Flex

<Group isVisible="{(a, b, c) => x + y < new Vector3(10, 10, 10)}"/>

<SubWindow type="typeof(Vector3)"/>

<Text label="$route.parameters['username {inter}' + expr ]"/>

String + Expression<string> + String

<Text>
    string goes {here}
</Text>

<Repeat list="[1, 2, 3]" as="(const)($i).ToString()"/>

<Text label="@(my string here) + $data.name"/>

<Stuff label="StaticClass.SomeValue.As<$root.genericArguments.T>"/>

<Thing position={$this.position + $root.position}/>

<RouterLink x-validate.username="{[Required, LengthAtLeast(4)]}"/>

*/