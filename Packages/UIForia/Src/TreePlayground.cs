
using System;
using System.Linq.Expressions;
using UnityEngine;

public class TestContext {

    public string val;
    public TestContext(string val) {
        this.val = val;
    }

}

public class TreePlayground {

    TestContext ctx = new TestContext("hello");
    
    public void MethodName() {
        Debug.Log(ctx.val);
    }

    public void Run() {
        ConstantExpression instance = Expression.Constant(this);
        
        MethodCallExpression expr = Expression.Call(
            instance, 
            GetType().GetMethod("MethodName")
        );

        Debug.Log(expr.ToString());
        Expression<Action> d = Expression.Lambda<Action>(expr);
        
        Action fn = d.Compile();

        fn();
        
    }

}