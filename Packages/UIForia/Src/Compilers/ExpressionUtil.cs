using System;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Extensions;
using UIForia.Util;

namespace UIForia.Compilers {

    internal static class ExpressionUtil {

        internal struct ParameterConversion {

            public readonly Expression expression;
            public readonly bool requiresConversion;
            public readonly MethodInfo userConversion;
            public readonly Type convertTo;
            public readonly object defaultValue;

            public ParameterConversion(Expression expression, bool requiresConversion, Type convertTo, MethodInfo userConversion = null, object defaultValue = null) {
                this.expression = expression;
                this.requiresConversion = requiresConversion;
                this.userConversion = userConversion;
                this.convertTo = convertTo;
                this.defaultValue = defaultValue;
            }

            public Expression Convert() {
                if (!requiresConversion) {
                    if (defaultValue != null) {
                        return Expression.Constant(defaultValue);
                    }

                    return expression;
                }

                return ExpressionFactory.Convert(expression, convertTo, userConversion);
            }

        }

        internal static ConstructorInfo SelectEligibleConstructor(Type type, Expression[] arguments, out StructList<ParameterConversion> winningConversions) {
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            if (constructors.Length == 0) {
                winningConversions = null;
                return null;
            }

            if (constructors.Length == 1) {
                winningConversions = null;
                if (CheckCtorCandidate(new ConstructorCandidate(constructors[0]), arguments, out int unused, out winningConversions)) {
                    return constructors[0];
                }

                return null;
            }

            ConstructorCandidate[] candidates = new ConstructorCandidate[constructors.Length];
            winningConversions = null;

            for (int i = 0; i < candidates.Length; i++) {
                candidates[i] = new ConstructorCandidate(constructors[i]);
            }

            int winner = -1;
            int winnerPoints = 0;
            for (int i = 0; i < constructors.Length; i++) {
                int candidatePoints;
                StructList<ParameterConversion> conversions;
                if (!CheckCtorCandidate(candidates[i], arguments, out candidatePoints, out conversions)) {
                    continue;
                }

                // todo -- handle the ambiguous case
                if (BestScoreSoFar(candidatePoints, winnerPoints)) {
                    winner = i;
                    winnerPoints = candidatePoints;
                    if (winningConversions != null) {
                        StructList<ParameterConversion>.Release(ref winningConversions);
                    }

                    winningConversions = conversions;
                }
            }

            if (winner != -1) {
                return constructors[winner];
            }

            return null;
        }

        public struct ConstructorCandidate {

            public ConstructorInfo constructorInfo;
            public ParameterInfo[] dependencies;

            public ConstructorCandidate(ConstructorInfo info) {
                this.constructorInfo = info;
                this.dependencies = info.GetParametersCached();
            }

        }

        private static bool BestScoreSoFar(int candidatePoints, int winnerPoints) {
            return winnerPoints < candidatePoints;
        }

        private static bool CheckCtorCandidate(ConstructorCandidate candidate, Expression[] context, out int candidatePoints, out StructList<ParameterConversion> conversions) {
            candidatePoints = 0;


            if (context.Length > candidate.dependencies.Length) {
                candidatePoints = 0;
                conversions = null;
                return false;
            }

            conversions = StructList<ParameterConversion>.Get();
            for (int i = 0; i < candidate.dependencies.Length; i++) {
                if (i < context.Length) {
                    Type paramType = candidate.dependencies[i].ParameterType;
                    Type argType = context[i].Type;
                    if (paramType == argType) {
                        candidatePoints += 100;
                        conversions.Add(new ParameterConversion(context[i], false, paramType));
                    }
                    else if (TypeUtil.HasIdentityPrimitiveOrNullableConversion(argType, paramType)) {
                        candidatePoints += 50;
                        conversions.Add(new ParameterConversion(context[i], true, paramType));
                    }
                    else if (TypeUtil.HasReferenceConversion(argType, paramType)) {
                        conversions.Add(new ParameterConversion(context[i], true, paramType));
                    }
                    else if (TypeUtil.TryGetUserDefinedCoercionMethod(argType, paramType, false, out MethodInfo info)) {
                        conversions.Add(new ParameterConversion(context[i], true, paramType, info));
                    }
                    else {
                        candidatePoints = 0;
                        return false;
                    }
                }
                else if (candidate.dependencies[i].HasDefaultValue) {
                    candidatePoints += 1;
                    conversions.Add(new ParameterConversion(null, false, null, null, candidate.dependencies[i].DefaultValue));
                }
                else {
                    candidatePoints = 0;
                    return false;
                }
            }

            return true;
        }

    }

}