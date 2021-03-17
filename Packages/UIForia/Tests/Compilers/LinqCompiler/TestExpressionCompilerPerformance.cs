using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastExpressionCompiler;
using NUnit.Framework;
using UIForia.Compilers;
using Unity.PerformanceTesting;

namespace UIForia.Test.NamespaceTest.SomeNamespace {
    public class TestExpressionCompilerPerformanceUtils {
        protected LambdaExpression CreateTestExpression(int statementCount) {
            LinqCompiler compiler = new LinqCompiler();
            compiler.SetSignature();
            TestLinqCompiler.StaticThing.value = 99;
            for (int i = 0; i < statementCount; ++i) {
                compiler.Statement("TestLinqCompiler.StaticThing.value = 12");    
            }
            
            return compiler.BuildLambda();
        }

        protected void RunMeasurement(Action action, int warmupCount = 5, int measurementCount = 100) {
            GC.Collect();
            
            Measure.Method(action)
                   .WarmupCount(warmupCount)
                   .MeasurementCount(measurementCount)
                   .CleanUp(GC.Collect)
                   .Run();

            GC.Collect();
        }
    }
    
    public class TestExpressionCompilerPerformance : TestExpressionCompilerPerformanceUtils {
        
        [Test, Performance]
        public void Measure0_10_stmts() {
            var expression = CreateTestExpression(10);
            RunMeasurement(() => { expression.Compile(); });
        }
        
        [Test, Performance]
        public void Measure1_50_stmts() {
            var expression = CreateTestExpression(50);
            RunMeasurement(() => { expression.Compile(); });
        }
        
        [Test, Performance]
        public void Measure2_100_stmts() {
            var expression = CreateTestExpression(100);
            RunMeasurement(() => { expression.Compile(); });
        }
        
        [Test, Performance]
        public void Measure3_500_stmts() {
            var expression = CreateTestExpression(500);
            RunMeasurement(() => { expression.Compile(); });
        }
        
        [Test, Performance]
        public void Measure4_1000_stmts() {
            var expression = CreateTestExpression(1000);
            RunMeasurement(() => { expression.Compile(); });
        }
        
        [Test, Performance]
        public void Measure5_5000_stmts() {
            var expression = CreateTestExpression(5000);
            RunMeasurement(() => { expression.Compile(); });
        }
        
        [Test, Performance]
        public void Measure6_10k_stmts() {
            var expression = CreateTestExpression(10000);
            RunMeasurement(() => { expression.Compile(); });
        }
        
        [Test, Performance]
        public void Measure7_50k_stmts() {
            var expression = CreateTestExpression(10000);
            RunMeasurement(() => { expression.Compile(); });
        }
        
        [Test, Performance]
        public void Measure8_100k_stmts() {
            var expression = CreateTestExpression(100000);
            RunMeasurement(() => { expression.Compile(); });
        }
        
        [Test, Performance]
        public void Measure9_500k_stmts() {
            var expression = CreateTestExpression(500000);
            RunMeasurement(() => { expression.Compile(); });
        }
    }
    
    public class TestExpressionCompilerPerformance_Fast : TestExpressionCompilerPerformanceUtils {
        
        [Test, Performance]
        public void Measure0_10_stmts_Fast() {
            var expression = CreateTestExpression(10);
            RunMeasurement(() => { expression.CompileFast(); });
        }
        
        [Test, Performance]
        public void Measure1_50_stmts_Fast() {
            var expression = CreateTestExpression(50);
            RunMeasurement(() => { expression.CompileFast(); });
        }
        
        [Test, Performance]
        public void Measure2_100_stmts_Fast() {
            var expression = CreateTestExpression(100);
            RunMeasurement(() => { expression.CompileFast(); });
        }
        
        [Test, Performance]
        public void Measure3_500_stmts_Fast() {
            var expression = CreateTestExpression(500);
            RunMeasurement(() => { expression.CompileFast(); });
        }
        
        [Test, Performance]
        public void Measure4_1000_stmts_Fast() {
            var expression = CreateTestExpression(1000);
            RunMeasurement(() => { expression.CompileFast(); });
        }
        
        [Test, Performance]
        public void Measure5_4000_stmts_Fast() {
            var expression = CreateTestExpression(4000);
            RunMeasurement(() => { expression.CompileFast(); });
        }
        
        [Test, Performance]
        public void Measure5_5000_stmts_Fast() {
            var expression = CreateTestExpression(5000);
            RunMeasurement(() => { expression.CompileFast(); });
        }
        
        [Test, Performance]
        public void Measure6_10k_stmts_Fast() {
            var expression = CreateTestExpression(10000);
            RunMeasurement(() => { expression.CompileFast(); });
        }
        
        [Test, Performance]
        public void Measure7_50k_stmts_Fast() {
            var expression = CreateTestExpression(10000);
            RunMeasurement(() => { expression.CompileFast(); });
        }
        
        [Test, Performance]
        public void Measure8_100k_stmts_Fast() {
            var expression = CreateTestExpression(100000);
            RunMeasurement(() => { expression.CompileFast(); });
        }
        
        [Test, Performance]
        public void Measure9_500k_stmts_Fast() {
            var expression = CreateTestExpression(500000);
            RunMeasurement(() => { expression.CompileFast(); });
        }
    }

    public class TestExpressionCompilerPerformance_2 : TestExpressionCompilerPerformanceUtils {
        
        private static LambdaExpression CreateLambdaExpression(int statementCount) {
            LinqCompiler compiler = new LinqCompiler();
            compiler.SetSignature<int>();
            TestLinqCompiler.StaticThing.value = 99;
            for (int i = 0; i < statementCount; ++i) {
                compiler.Statement("TestLinqCompiler.StaticThing.value = 12");    
            }

            compiler.Return("1");
            return compiler.BuildLambda();
        }
        public LambdaExpression CreateTestExpression(int lambdaCount, int statementCount) {
            var expressions = new List<Expression>();
            
            for (int i = 0; i < lambdaCount; ++i) {
                var lambdaExpression = CreateLambdaExpression(statementCount);
                var callLambda = Expression.Invoke(lambdaExpression);
                expressions.Add(callLambda);
            }
            
            BlockExpression expression = Expression.Block(expressions);
            return Expression.Lambda(expression);
        }

        [Test, Performance]
        public void TestManySmall_Fast() {
            var expression = CreateTestExpression(100, 50);
            RunMeasurement(() => { expression.CompileFast(); });
        }
        
        [Test, Performance]
        public void TestOneBig_Fast() {
            var expression = CreateTestExpression(1, 5000);
            RunMeasurement(() => { expression.CompileFast(); });
        }
    }

    public class TestExpressionCompilerPerformanceUtils_MT : TestExpressionCompilerPerformanceUtils {

        private LambdaExpression[] CreateTestExpressionSet(int numberOfExpressions, int statementCount) {
            LambdaExpression[] result = new LambdaExpression[numberOfExpressions];
            for (int i = 0; i < numberOfExpressions; ++i) {
                result[i] = CreateTestExpression(statementCount);
            }

            return result;
        }

        private void RunMeasurementMT(int expressionCount, int statementCount) {
            LambdaExpression[] testSet = CreateTestExpressionSet(expressionCount, statementCount);
            RunMeasurement(() => {
                Parallel.ForEach(testSet, expression => expression.CompileFast());
            }, warmupCount:8);
        }
        
        [Test, Performance]
        public void Measure_1000_stmts_2_exprs() {
            RunMeasurementMT(2, 1000);
        }
        
        [Test, Performance]
        public void Measure_1000_stmts_4_exprs() {
            RunMeasurementMT(4, 1000);
        }

        [Test, Performance]
        public void Measure_1000_stmts_8_exprs() {
            RunMeasurementMT(8, 1000);
        }
        
        [Test, Performance]
        public void Measure_4000_stmts_2_exprs() {
            RunMeasurementMT(2, 4000);
        }

        [Test, Performance]
        public void Measure_5000_stmts_2_exprs() {
            RunMeasurementMT(2, 5000);
        }
        
        [Test, Performance]
        public void Measure_5000_stmts_4_exprs() {
            RunMeasurementMT(4, 5000);
        }

        [Test, Performance]
        public void Measure_5000_stmts_10_exprs() {
            RunMeasurementMT(10, 5000);
        }

        [Test, Performance]
        public void Measure_5000_stmts_20_exprs() {
            RunMeasurementMT(20, 5000);
        }
        
        [Test, Performance]
        public void Measure_5000_stmts_40_exprs() {
            RunMeasurementMT(40, 5000);
        }

        #region Workload test
        // test there total workload = 4000
        
        [Test, Performance]
        public void Measure_total_4000_stmts_1_exprs() {
            RunMeasurementMT(1, 4000);
        }
        
        [Test, Performance]
        public void Measure_total_4000_stmts_2_exprs() {
            RunMeasurementMT(2, 2000);
        }
        
        [Test, Performance]
        public void Measure_total_4000_stmts_3_exprs() {
            RunMeasurementMT(3, 1333);
        }
        
        [Test, Performance]
        public void Measure_total_4000_stmts_4_exprs() {
            RunMeasurementMT(4, 1000);
        }
        
        [Test, Performance]
        public void Measure_total_4000_stmts_5_exprs() {
            RunMeasurementMT(5, 800);
        }
        
        #endregion
    }
}