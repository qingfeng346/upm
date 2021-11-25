﻿using Scorpio.Instruction;
using Scorpio.Exception;
using Scorpio.Function;
using Scorpio.Tools;
namespace Scorpio.Runtime {

    //执行命令
    //注意事项:
    //所有调用另一个程序集的地方 都要new一个新的 否则递归调用会相互影响
    public class ScriptContext {
        private const int ParameterLength = 128;        //函数参数最大数量
        private const int ValueCacheLength = 128;       //函数最大调用层级,超过会堆栈溢出
        private const int StackValueLength = 256;       //堆栈数据最大数量
        private const int VariableValueLength = 128;    //局部变量最大数量
        protected static ScriptValue[] Parameters = new ScriptValue[ParameterLength];               //函数调用共用数组
        protected static ScriptValue[][] VariableValues = new ScriptValue[ValueCacheLength][];      //局部变量数据
        protected static ScriptValue[][] StackValues = new ScriptValue[ValueCacheLength][];         //堆栈数据
        protected static int VariableValueIndex = 0;
        static ScriptContext() {
            for (var i = 0; i < StackValues.Length; ++i) {
                StackValues[i] = new ScriptValue[StackValueLength];
            }
            for (var i = 0; i < VariableValues.Length; ++i) {
                VariableValues[i] = new ScriptValue[VariableValueLength];
            }
        }

        public readonly Script m_script;                            //脚本类
        private readonly ScriptGlobal m_global;                     //global

        private readonly double[] constDouble;                      //double常量
        private readonly long[] constLong;                          //long常量
        private readonly string[] constString;                      //string常量
        private readonly ScriptContext[] constContexts;             //所有定义的函数
        private readonly ScriptClassData[] constClasses;            //定义所有的类
        public readonly int internalCount;                          //内部变量数量

        private readonly string m_Breviary;                         //摘要
        private readonly ScriptFunctionData m_FunctionData;         //函数数据
        private readonly ScriptInstruction[] m_scriptInstructions;  //指令集

        public ScriptContext(Script script, string breviary, ScriptFunctionData functionData, double[] constDouble, long[] constLong, string[] constString, ScriptContext[] constContexts, ScriptClassData[] constClasses) {
            m_script = script;
            m_global = script.Global;
            this.constDouble = constDouble;
            this.constLong = constLong;
            this.constString = constString;
            this.constContexts = constContexts;
            this.constClasses = constClasses;
            this.internalCount = functionData.internalCount;

            m_Breviary = breviary;
            m_FunctionData = functionData;
            m_scriptInstructions = functionData.scriptInstructions;
        }
        public ScriptValue Execute(ScriptValue thisObject, ScriptValue[] args, int length, InternalValue[] internalValues) {
#if SCORPIO_DEBUG
            // Logger.debug($"执行命令 =>\n{m_FunctionData.ToString(constDouble, constLong, constString)}");
#endif
            var variableObjects = VariableValues[VariableValueIndex];   //局部变量
            var stackObjects = StackValues[VariableValueIndex++];       //堆栈数据
            variableObjects[0] = thisObject;
            InternalValue[] internalObjects = null;
            if (internalCount > 0) {
                internalObjects = new InternalValue[internalCount];     //内部变量，有外部引用
                if (internalValues == null) {
                    for (int i = 0; i < internalCount; ++i) {
                        internalObjects[i] = new InternalValue();
                    }
                } else {
                    for (int i = 0; i < internalCount; ++i) {
                        internalObjects[i] = internalValues[i] ?? new InternalValue();
                    }
                }
            }
            var stackIndex = -1;                                        //堆栈索引
            var parameterCount = m_FunctionData.parameterCount;         //参数数量
            //是否是变长参数
            if (m_FunctionData.param) {
                var array = new ScriptArray(m_script);
                for (var i = parameterCount - 1; i < length; ++i) {
                    array.Add(args[i]);
                }
                stackObjects[++stackIndex].scriptValue = array;
                stackObjects[stackIndex].valueType = ScriptValue.scriptValueType;
                for (var i = parameterCount - 2; i >= 0; --i) {
                    stackObjects[++stackIndex] = i >= length ? ScriptValue.Null : args[i];
                }
            } else {
                for (var i = parameterCount - 1; i >= 0; --i) {
                    stackObjects[++stackIndex] = i >= length ? ScriptValue.Null : args[i];
                }
            }
            var parent = ScriptValue.Null;
            var parameters = Parameters;                                //传递参数
            var iInstruction = 0;                                       //当前执行命令索引
            var iInstructionCount = m_scriptInstructions.Length;        //指令数量
            byte valueType;
            int index;
            ScriptInstruction instruction = null;
            try {
                while (iInstruction < iInstructionCount) {
                    instruction = m_scriptInstructions[iInstruction++];
                    var opvalue = instruction.opvalue;
                    var opcode = instruction.opcode;
                    switch (instruction.optype) {
                        case OpcodeType.Load:
                            switch (opcode) {
                                case Opcode.LoadConstDouble: {
                                    stackObjects[++stackIndex].doubleValue = constDouble[opvalue];
                                    stackObjects[stackIndex].valueType = ScriptValue.doubleValueType;
                                    continue;
                                } 
                                case Opcode.LoadConstString: {
                                    stackObjects[++stackIndex].stringValue = constString[opvalue];
                                    stackObjects[stackIndex].valueType = ScriptValue.stringValueType;
                                    continue;
                                }
                                case Opcode.LoadConstNull: stackObjects[++stackIndex].valueType = ScriptValue.nullValueType; continue;
                                case Opcode.LoadConstTrue: stackObjects[++stackIndex].valueType = ScriptValue.trueValueType; continue;
                                case Opcode.LoadConstFalse: stackObjects[++stackIndex].valueType = ScriptValue.falseValueType; continue;
                                case Opcode.LoadConstLong: {
                                    stackObjects[++stackIndex].longValue = constLong[opvalue];
                                    stackObjects[stackIndex].valueType = ScriptValue.longValueType;
                                    continue;
                                }
                                case Opcode.LoadLocal: {
                                    switch (variableObjects[opvalue].valueType) {
                                        case ScriptValue.scriptValueType:
                                            stackObjects[++stackIndex].scriptValue = variableObjects[opvalue].scriptValue;
                                            stackObjects[stackIndex].valueType = ScriptValue.scriptValueType;
                                            continue;
                                        case ScriptValue.doubleValueType:
                                            stackObjects[++stackIndex].doubleValue = variableObjects[opvalue].doubleValue;
                                            stackObjects[stackIndex].valueType = ScriptValue.doubleValueType;
                                            continue;
                                        case ScriptValue.nullValueType: stackObjects[++stackIndex].valueType = ScriptValue.nullValueType; continue;
                                        case ScriptValue.trueValueType: stackObjects[++stackIndex].valueType = ScriptValue.trueValueType; continue;
                                        case ScriptValue.falseValueType: stackObjects[++stackIndex].valueType = ScriptValue.falseValueType; continue;
                                        case ScriptValue.stringValueType:
                                            stackObjects[++stackIndex].stringValue = variableObjects[opvalue].stringValue;
                                            stackObjects[stackIndex].valueType = ScriptValue.stringValueType;
                                            continue;
                                        case ScriptValue.longValueType:
                                            stackObjects[++stackIndex].longValue = variableObjects[opvalue].longValue;
                                            stackObjects[stackIndex].valueType = ScriptValue.longValueType;
                                            continue;
                                        case ScriptValue.objectValueType:
                                            stackObjects[++stackIndex].objectValue = variableObjects[opvalue].objectValue;
                                            stackObjects[stackIndex].valueType = ScriptValue.objectValueType;
                                            continue;
                                        default: throw new ExecutionException($"LoadLocal : 未知错误数据类型 : {variableObjects[opvalue].ValueTypeName}");
                                    }
                                }
                                case Opcode.LoadInternal: {
                                    switch (internalObjects[opvalue].value.valueType) {
                                        case ScriptValue.scriptValueType:
                                            stackObjects[++stackIndex].scriptValue = internalObjects[opvalue].value.scriptValue;
                                            stackObjects[stackIndex].valueType = ScriptValue.scriptValueType;
                                            continue;
                                        case ScriptValue.doubleValueType:
                                            stackObjects[++stackIndex].doubleValue = internalObjects[opvalue].value.doubleValue;
                                            stackObjects[stackIndex].valueType = ScriptValue.doubleValueType;
                                            continue;
                                        case ScriptValue.nullValueType: stackObjects[++stackIndex].valueType = ScriptValue.nullValueType; continue;
                                        case ScriptValue.trueValueType: stackObjects[++stackIndex].valueType = ScriptValue.trueValueType; continue;
                                        case ScriptValue.falseValueType: stackObjects[++stackIndex].valueType = ScriptValue.falseValueType; continue;
                                        case ScriptValue.stringValueType:
                                            stackObjects[++stackIndex].stringValue = internalObjects[opvalue].value.stringValue;
                                            stackObjects[stackIndex].valueType = ScriptValue.stringValueType;
                                            continue;
                                        case ScriptValue.longValueType:
                                            stackObjects[++stackIndex].longValue = internalObjects[opvalue].value.longValue;
                                            stackObjects[stackIndex].valueType = ScriptValue.longValueType;
                                            continue;
                                        case ScriptValue.objectValueType:
                                            stackObjects[++stackIndex].objectValue = internalObjects[opvalue].value.objectValue;
                                            stackObjects[stackIndex].valueType = ScriptValue.objectValueType;
                                            continue;
                                        default: throw new ExecutionException("LoadInternal : 未知错误数据类型 : " + internalObjects[opvalue].value.valueType);
                                    }
                                }
                                case Opcode.LoadValue: {
                                    parent = stackObjects[stackIndex];
                                    stackObjects[stackIndex] = stackObjects[stackIndex].GetValueByIndex(opvalue, m_script);
                                    continue;
                                }
                                case Opcode.LoadValueString: {
                                    parent = stackObjects[stackIndex];
                                    stackObjects[stackIndex] = stackObjects[stackIndex].GetValue(constString[opvalue], m_script);
                                    continue;
                                }
                                case Opcode.LoadValueObject: {
                                    parent = stackObjects[stackIndex - 1];
                                    switch (stackObjects[stackIndex].valueType) {
                                        case ScriptValue.stringValueType: stackObjects[stackIndex - 1] = stackObjects[stackIndex - 1].GetValue(stackObjects[stackIndex].stringValue, m_script); break;
                                        case ScriptValue.doubleValueType: stackObjects[stackIndex - 1] = stackObjects[stackIndex - 1].GetValue(stackObjects[stackIndex].doubleValue); break;
                                        case ScriptValue.longValueType: stackObjects[stackIndex - 1] = stackObjects[stackIndex - 1].GetValue(stackObjects[stackIndex].longValue); break;
                                        case ScriptValue.scriptValueType: stackObjects[stackIndex - 1] = stackObjects[stackIndex - 1].GetValue(stackObjects[stackIndex].scriptValue); break;
                                        case ScriptValue.objectValueType: stackObjects[stackIndex - 1] = stackObjects[stackIndex - 1].GetValue(stackObjects[stackIndex].objectValue); break;
                                        default: throw new ExecutionException($"不支持当前类型获取变量 LoadValueObject : {stackObjects[stackIndex].ValueTypeName}");
                                    }
                                    --stackIndex;
                                    continue;
                                }
                                case Opcode.LoadValueObjectDup: {
                                    parent = stackObjects[stackIndex - 1];
                                    switch (stackObjects[stackIndex].valueType) {
                                        case ScriptValue.stringValueType: stackObjects[stackIndex + 1] = stackObjects[stackIndex - 1].GetValue(stackObjects[stackIndex].stringValue, m_script); break;
                                        case ScriptValue.doubleValueType: stackObjects[stackIndex + 1] = stackObjects[stackIndex - 1].GetValue(stackObjects[stackIndex].doubleValue); break;
                                        case ScriptValue.longValueType: stackObjects[stackIndex + 1] = stackObjects[stackIndex - 1].GetValue(stackObjects[stackIndex].longValue); break;
                                        case ScriptValue.scriptValueType: stackObjects[stackIndex + 1] = stackObjects[stackIndex - 1].GetValue(stackObjects[stackIndex].scriptValue); break;
                                        case ScriptValue.objectValueType: stackObjects[stackIndex + 1] = stackObjects[stackIndex - 1].GetValue(stackObjects[stackIndex].objectValue); break;
                                        default: throw new ExecutionException($"不支持当前类型获取变量 LoadValueObjectDup : {stackObjects[stackIndex].ValueTypeName}");
                                    }
                                    ++stackIndex;
                                    continue;
                                }
                                case Opcode.LoadGlobal: {
                                    stackObjects[++stackIndex] = m_global.GetValueByIndex(opvalue);
                                    continue;
                                }
                                case Opcode.LoadGlobalString: {
                                    stackObjects[++stackIndex] = m_global.GetValue(constString[opvalue]);
                                    instruction.SetOpcode(Opcode.LoadGlobal, m_global.GetIndex(constString[opvalue]));
                                    continue;
                                }
                                case Opcode.CopyStackTop: {
                                    stackObjects[++stackIndex] = stackObjects[stackIndex - 1];
                                    continue;
                                }
                                case Opcode.CopyStackTopIndex: {
                                    stackObjects[++stackIndex] = stackObjects[stackIndex - opvalue - 1];
                                    continue;
                                }
                            }
                            continue;
                        case OpcodeType.Store:
                            switch (opcode) {
                                //-------------下面为 = *= -= 等赋值操作, 压入计算结果
                                case Opcode.StoreLocalAssign: {
                                    switch (stackObjects[stackIndex].valueType) {
                                        case ScriptValue.scriptValueType:
                                            variableObjects[opvalue].scriptValue = stackObjects[stackIndex].scriptValue;
                                            variableObjects[opvalue].valueType = ScriptValue.scriptValueType;
                                            continue;
                                        case ScriptValue.doubleValueType:
                                            variableObjects[opvalue].doubleValue = stackObjects[stackIndex].doubleValue;
                                            variableObjects[opvalue].valueType = ScriptValue.doubleValueType;
                                            continue;
                                        case ScriptValue.nullValueType: variableObjects[opvalue].valueType = ScriptValue.nullValueType; continue;
                                        case ScriptValue.trueValueType: variableObjects[opvalue].valueType = ScriptValue.trueValueType; continue;
                                        case ScriptValue.falseValueType: variableObjects[opvalue].valueType = ScriptValue.falseValueType; continue;
                                        case ScriptValue.stringValueType:
                                            variableObjects[opvalue].stringValue = stackObjects[stackIndex].stringValue;
                                            variableObjects[opvalue].valueType = ScriptValue.stringValueType;
                                            continue;
                                        case ScriptValue.longValueType:
                                            variableObjects[opvalue].longValue = stackObjects[stackIndex].longValue;
                                            variableObjects[opvalue].valueType = ScriptValue.longValueType;
                                            continue;
                                        case ScriptValue.objectValueType:
                                            variableObjects[opvalue].objectValue = stackObjects[stackIndex].objectValue;
                                            variableObjects[opvalue].valueType = ScriptValue.objectValueType;
                                            continue;
                                        default: throw new ExecutionException($"StoreLocalAssign : 未知错误数据类型 : {stackObjects[stackIndex].ValueTypeName}");
                                    }
                                }
                                case Opcode.StoreInternalAssign: {
                                    switch (stackObjects[stackIndex].valueType) {
                                        case ScriptValue.scriptValueType:
                                            internalObjects[opvalue].value.scriptValue = stackObjects[stackIndex].scriptValue;
                                            internalObjects[opvalue].value.valueType = ScriptValue.scriptValueType;
                                            continue;
                                        case ScriptValue.doubleValueType:
                                            internalObjects[opvalue].value.doubleValue = stackObjects[stackIndex].doubleValue;
                                            internalObjects[opvalue].value.valueType = ScriptValue.doubleValueType;
                                            continue;
                                        case ScriptValue.nullValueType: internalObjects[opvalue].value.valueType = ScriptValue.nullValueType; continue;
                                        case ScriptValue.trueValueType: internalObjects[opvalue].value.valueType = ScriptValue.trueValueType; continue;
                                        case ScriptValue.falseValueType: internalObjects[opvalue].value.valueType = ScriptValue.falseValueType; continue;
                                        case ScriptValue.stringValueType:
                                            internalObjects[opvalue].value.stringValue = stackObjects[stackIndex].stringValue;
                                            internalObjects[opvalue].value.valueType = ScriptValue.stringValueType;
                                            continue;
                                        case ScriptValue.longValueType:
                                            internalObjects[opvalue].value.longValue = stackObjects[stackIndex].longValue;
                                            internalObjects[opvalue].value.valueType = ScriptValue.longValueType;
                                            continue;
                                        case ScriptValue.objectValueType:
                                            internalObjects[opvalue].value.objectValue = stackObjects[stackIndex].objectValue;
                                            internalObjects[opvalue].value.valueType = ScriptValue.objectValueType;
                                            continue;
                                        default: throw new ExecutionException("StoreInternalAssign : 未知错误数据类型 : " + stackObjects[stackIndex].valueType);
                                    }
                                }
                                case Opcode.StoreValueStringAssign: {
                                    index = stackIndex;
                                    stackObjects[stackIndex - 1].SetValue(constString[opvalue], stackObjects[stackIndex]);
                                    stackObjects[--stackIndex] = stackObjects[index];
                                    continue;
                                }
                                case Opcode.StoreValueObjectAssign: {
                                    index = stackIndex;
                                    switch (stackObjects[stackIndex - 1].valueType) {
                                        case ScriptValue.stringValueType: stackObjects[stackIndex - 2].SetValue(stackObjects[stackIndex - 1].stringValue, stackObjects[stackIndex]); break;
                                        case ScriptValue.doubleValueType: stackObjects[stackIndex - 2].SetValue(stackObjects[stackIndex - 1].doubleValue, stackObjects[stackIndex]); break;
                                        case ScriptValue.longValueType: stackObjects[stackIndex - 2].SetValue(stackObjects[stackIndex - 1].longValue, stackObjects[stackIndex]); break;
                                        case ScriptValue.scriptValueType: stackObjects[stackIndex - 2].SetValue(stackObjects[stackIndex - 1].scriptValue, stackObjects[stackIndex]); break;
                                        case ScriptValue.objectValueType: stackObjects[stackIndex - 2].SetValue(stackObjects[stackIndex - 1].objectValue, stackObjects[stackIndex]); break;
                                        default: throw new ExecutionException($"不支持当前类型设置变量 : {stackObjects[stackIndex - 1].ValueTypeName}");
                                    }
                                    stackObjects[stackIndex -= 2] = stackObjects[index];
                                    continue;
                                }
                                case Opcode.StoreGlobalAssign: m_global.SetValueByIndex(opvalue, stackObjects[stackIndex]); continue;
                                case Opcode.StoreGlobalStringAssign: {
                                    m_global.SetValue(constString[opvalue], stackObjects[stackIndex]);
                                    instruction.SetOpcode(Opcode.StoreGlobalAssign, m_global.GetIndex(constString[opvalue]));
                                    continue;
                                }
                                case Opcode.StoreValueAssign: {
                                    index = stackIndex;
                                    stackObjects[stackIndex - 1].SetValueByIndex(opvalue, stackObjects[stackIndex]);
                                    stackObjects[--stackIndex] = stackObjects[index];
                                    continue;
                                }


                                //-----------------下面为普通赋值操作 不压入结果
                                case Opcode.StoreLocal: {
                                    index = stackIndex--;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.scriptValueType:
                                            variableObjects[opvalue].scriptValue = stackObjects[index].scriptValue;
                                            variableObjects[opvalue].valueType = ScriptValue.scriptValueType;
                                            continue;
                                        case ScriptValue.doubleValueType:
                                            variableObjects[opvalue].doubleValue = stackObjects[index].doubleValue;
                                            variableObjects[opvalue].valueType = ScriptValue.doubleValueType;
                                            continue;
                                        case ScriptValue.nullValueType: variableObjects[opvalue].valueType = ScriptValue.nullValueType; continue;
                                        case ScriptValue.trueValueType: variableObjects[opvalue].valueType = ScriptValue.trueValueType; continue;
                                        case ScriptValue.falseValueType: variableObjects[opvalue].valueType = ScriptValue.falseValueType; continue;
                                        case ScriptValue.stringValueType:
                                            variableObjects[opvalue].stringValue = stackObjects[index].stringValue;
                                            variableObjects[opvalue].valueType = ScriptValue.stringValueType;
                                            continue;
                                        case ScriptValue.longValueType:
                                            variableObjects[opvalue].longValue = stackObjects[index].longValue;
                                            variableObjects[opvalue].valueType = ScriptValue.longValueType;
                                            continue;
                                        case ScriptValue.objectValueType:
                                            variableObjects[opvalue].objectValue = stackObjects[index].objectValue;
                                            variableObjects[opvalue].valueType = ScriptValue.objectValueType;
                                            continue;
                                        default: throw new ExecutionException($"StoreLocal : 未知错误数据类型 : {stackObjects[stackIndex].ValueTypeName}");
                                    }
                                }
                                case Opcode.StoreInternal: {
                                    index = stackIndex--;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.scriptValueType:
                                            internalObjects[opvalue].value.scriptValue = stackObjects[index].scriptValue;
                                            internalObjects[opvalue].value.valueType = ScriptValue.scriptValueType;
                                            continue;
                                        case ScriptValue.doubleValueType:
                                            internalObjects[opvalue].value.doubleValue = stackObjects[index].doubleValue;
                                            internalObjects[opvalue].value.valueType = ScriptValue.doubleValueType;
                                            continue;
                                        case ScriptValue.nullValueType: internalObjects[opvalue].value.valueType = ScriptValue.nullValueType; continue;
                                        case ScriptValue.trueValueType: internalObjects[opvalue].value.valueType = ScriptValue.trueValueType; continue;
                                        case ScriptValue.falseValueType: internalObjects[opvalue].value.valueType = ScriptValue.falseValueType; continue;
                                        case ScriptValue.stringValueType:
                                            internalObjects[opvalue].value.stringValue = stackObjects[index].stringValue;
                                            internalObjects[opvalue].value.valueType = ScriptValue.stringValueType;
                                            continue;
                                        case ScriptValue.longValueType:
                                            internalObjects[opvalue].value.longValue = stackObjects[index].longValue;
                                            internalObjects[opvalue].value.valueType = ScriptValue.longValueType;
                                            continue;
                                        case ScriptValue.objectValueType:
                                            internalObjects[opvalue].value.objectValue = stackObjects[index].objectValue;
                                            internalObjects[opvalue].value.valueType = ScriptValue.objectValueType;
                                            continue;
                                        default: throw new ExecutionException("StoreInternal : 未知错误数据类型 : " + stackObjects[index].valueType);
                                    }
                                }
                                case Opcode.StoreValueString: {
                                    stackObjects[stackIndex - 1].SetValue(constString[opvalue], stackObjects[stackIndex]);
                                    stackIndex -= 2;
                                    continue;
                                }
                                case Opcode.StoreGlobal: m_global.SetValueByIndex(opvalue, stackObjects[stackIndex--]); continue;
                                case Opcode.StoreGlobalString: {
                                    m_global.SetValue(constString[opvalue], stackObjects[stackIndex--]);
                                    instruction.SetOpcode(Opcode.StoreGlobal, m_global.GetIndex(constString[opvalue]));
                                    continue;
                                }
                            }
                            continue;
                        case OpcodeType.Compute:
                            switch (opcode) {
                                case Opcode.Plus: {
                                    index = stackIndex - 1;
                                    valueType = stackObjects[index].valueType;
                                    switch (valueType) {
                                        case ScriptValue.stringValueType: {
                                            stackObjects[index].stringValue += stackObjects[stackIndex].ToString();
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index] = stackObjects[index].scriptValue.Plus(stackObjects[stackIndex]);
                                            --stackIndex;
                                            continue;
                                        }
                                        default: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.stringValueType) {
                                                stackObjects[index].stringValue = stackObjects[index].ToString() + stackObjects[stackIndex].stringValue;
                                                stackObjects[index].valueType = ScriptValue.stringValueType;
                                            } else {
                                                if (valueType == ScriptValue.doubleValueType) {
                                                    if (stackObjects[stackIndex].valueType == ScriptValue.doubleValueType) {
                                                        stackObjects[index].doubleValue += stackObjects[stackIndex].doubleValue;
                                                    } else {
                                                        stackObjects[index].doubleValue += stackObjects[stackIndex].ToDouble();
                                                    }
                                                } else if (valueType == ScriptValue.longValueType) {
                                                    switch (stackObjects[stackIndex].valueType) {
                                                        case ScriptValue.longValueType:
                                                            stackObjects[index].longValue += stackObjects[stackIndex].longValue;
                                                            break;
                                                        case ScriptValue.doubleValueType:
                                                            stackObjects[index].doubleValue = stackObjects[index].longValue + stackObjects[stackIndex].doubleValue;
                                                            stackObjects[index].valueType = ScriptValue.doubleValueType;
                                                            break;
                                                        default:
                                                            stackObjects[index].longValue += stackObjects[stackIndex].ToLong();
                                                            break;
                                                    }
                                                } else {
                                                    throw new ExecutionException($"【+】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                                }
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                    }
                                }
                                case Opcode.Minus: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.doubleValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.doubleValueType) {
                                                stackObjects[index].doubleValue -= stackObjects[stackIndex].doubleValue;
                                            } else {
                                                stackObjects[index].doubleValue -= stackObjects[stackIndex].ToDouble();
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index] = stackObjects[index].scriptValue.Minus(stackObjects[stackIndex]);
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.longValueType: {
                                            switch (stackObjects[stackIndex].valueType) {
                                                case ScriptValue.longValueType:
                                                    stackObjects[index].longValue -= stackObjects[stackIndex].longValue;
                                                    break;
                                                case ScriptValue.doubleValueType:
                                                    stackObjects[index].doubleValue = stackObjects[index].longValue - stackObjects[stackIndex].doubleValue;
                                                    stackObjects[index].valueType = ScriptValue.doubleValueType;
                                                    break;
                                                default:
                                                    stackObjects[index].longValue += stackObjects[stackIndex].ToLong();
                                                    break;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【-】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.Multiply: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.doubleValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.doubleValueType) {
                                                stackObjects[index].doubleValue *= stackObjects[stackIndex].doubleValue;
                                            } else {
                                                stackObjects[index].doubleValue *= stackObjects[stackIndex].ToDouble();
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index] = stackObjects[index].scriptValue.Multiply(stackObjects[stackIndex]);
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.longValueType: {
                                            switch (stackObjects[stackIndex].valueType) {
                                                case ScriptValue.longValueType:
                                                    stackObjects[index].longValue *= stackObjects[stackIndex].longValue;
                                                    break;
                                                case ScriptValue.doubleValueType:
                                                    stackObjects[index].doubleValue = stackObjects[index].longValue * stackObjects[stackIndex].doubleValue;
                                                    stackObjects[index].valueType = ScriptValue.doubleValueType;
                                                    break;
                                                default:
                                                    stackObjects[index].longValue *= stackObjects[stackIndex].ToLong();
                                                    break;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【*】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.Divide: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.doubleValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.doubleValueType) {
                                                stackObjects[index].doubleValue /= stackObjects[stackIndex].doubleValue;
                                            } else {
                                                stackObjects[index].doubleValue /= stackObjects[stackIndex].ToDouble();
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index] = stackObjects[index].scriptValue.Divide(stackObjects[stackIndex]);
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.longValueType: {
                                            switch (stackObjects[stackIndex].valueType) {
                                                case ScriptValue.longValueType:
                                                    stackObjects[index].longValue /= stackObjects[stackIndex].longValue;
                                                    break;
                                                case ScriptValue.doubleValueType:
                                                    stackObjects[index].doubleValue = stackObjects[index].longValue / stackObjects[stackIndex].doubleValue;
                                                    stackObjects[index].valueType = ScriptValue.doubleValueType;
                                                    break;
                                                default:
                                                    stackObjects[index].longValue /= stackObjects[stackIndex].ToLong();
                                                    break;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【/】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.Modulo: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.doubleValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.doubleValueType) {
                                                stackObjects[index].doubleValue %= stackObjects[stackIndex].doubleValue;
                                            } else {
                                                stackObjects[index].doubleValue %= stackObjects[stackIndex].ToDouble();
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index] = stackObjects[index].scriptValue.Modulo(stackObjects[stackIndex]);
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.longValueType: {
                                            switch (stackObjects[stackIndex].valueType) {
                                                case ScriptValue.longValueType:
                                                    stackObjects[index].longValue %= stackObjects[stackIndex].longValue;
                                                    break;
                                                case ScriptValue.doubleValueType:
                                                    stackObjects[index].doubleValue = stackObjects[index].longValue % stackObjects[stackIndex].doubleValue;
                                                    stackObjects[index].valueType = ScriptValue.doubleValueType;
                                                    break;
                                                default:
                                                    stackObjects[index].longValue %= stackObjects[stackIndex].ToLong();
                                                    break;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【%】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.InclusiveOr: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.longValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.longValueType) {
                                                stackObjects[index].longValue |= stackObjects[stackIndex].longValue;
                                            } else {
                                                stackObjects[index].longValue |= stackObjects[stackIndex].ToLong();
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index] = stackObjects[index].scriptValue.InclusiveOr(stackObjects[stackIndex]);
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【|】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.Combine: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.longValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.longValueType) {
                                                stackObjects[index].longValue &= stackObjects[stackIndex].longValue;
                                            } else {
                                                stackObjects[index].longValue &= stackObjects[stackIndex].ToLong();
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index] = stackObjects[index].scriptValue.Combine(stackObjects[stackIndex]);
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【&】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.XOR: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.longValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.longValueType) {
                                                stackObjects[index].longValue ^= stackObjects[stackIndex].longValue;
                                            } else {
                                                stackObjects[index].longValue ^= stackObjects[stackIndex].ToLong();
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index] = stackObjects[index].scriptValue.XOR(stackObjects[stackIndex]);
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【^】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.Shi: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.longValueType: {
                                            stackObjects[index].longValue <<= stackObjects[stackIndex].ToInt32();
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index] = stackObjects[index].scriptValue.Shi(stackObjects[stackIndex]);
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【<<】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.Shr: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.longValueType: {
                                            stackObjects[index].longValue >>= stackObjects[stackIndex].ToInt32();
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index] = stackObjects[index].scriptValue.Shr(stackObjects[stackIndex]);
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【>>】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.FlagNot: {
                                    switch (stackObjects[stackIndex].valueType) {
                                        case ScriptValue.trueValueType: 
                                            stackObjects[stackIndex].valueType = ScriptValue.falseValueType; 
                                            continue;
                                        case ScriptValue.falseValueType:
                                        case ScriptValue.nullValueType:
                                            stackObjects[stackIndex].valueType = ScriptValue.trueValueType; 
                                            continue;
                                        default: throw new ExecutionException($"当前数据类型不支持取反操作 : {stackObjects[stackIndex].ValueTypeName}");
                                    }
                                }
                                case Opcode.FlagMinus: {
                                    switch (stackObjects[stackIndex].valueType) {
                                        case ScriptValue.doubleValueType: stackObjects[stackIndex].doubleValue = -stackObjects[stackIndex].doubleValue; continue;
                                        case ScriptValue.longValueType: stackObjects[stackIndex].longValue = -stackObjects[stackIndex].longValue; continue;
                                        default: throw new ExecutionException($"当前数据类型不支持取负操作 : {stackObjects[stackIndex].ValueTypeName}");
                                    }
                                }
                                case Opcode.FlagNegative: {
                                    if (stackObjects[stackIndex].valueType == ScriptValue.longValueType) {
                                        stackObjects[stackIndex].longValue = ~stackObjects[stackIndex].longValue;
                                        continue;
                                    } else {
                                        throw new ExecutionException($"当前数据类型不支持取非操作 : {stackObjects[stackIndex].ValueTypeName}");
                                    }
                                }
                            }
                            continue;
                        case OpcodeType.Compare:
                            switch (opcode) {
                                case Opcode.Less: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.doubleValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.doubleValueType) {
                                                stackObjects[index].valueType = stackObjects[index].doubleValue < stackObjects[stackIndex].doubleValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            } else {
                                                stackObjects[index].valueType = stackObjects[index].doubleValue < stackObjects[stackIndex].ToDouble() ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.longValueType: {
                                            switch (stackObjects[stackIndex].valueType) {
                                                case ScriptValue.longValueType:
                                                    stackObjects[index].valueType = stackObjects[index].longValue < stackObjects[stackIndex].longValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                                case ScriptValue.doubleValueType:
                                                    stackObjects[index].valueType = stackObjects[index].longValue < stackObjects[stackIndex].doubleValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                                default:
                                                    stackObjects[index].valueType = stackObjects[index].longValue < stackObjects[stackIndex].ToLong() ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index].valueType = stackObjects[index].scriptValue.Less(stackObjects[stackIndex]) ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【<】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.LessOrEqual: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.doubleValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.doubleValueType) {
                                                stackObjects[index].valueType = stackObjects[index].doubleValue <= stackObjects[stackIndex].doubleValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            } else {
                                                stackObjects[index].valueType = stackObjects[index].doubleValue <= stackObjects[stackIndex].ToDouble() ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.longValueType: {
                                            switch (stackObjects[stackIndex].valueType) {
                                                case ScriptValue.longValueType:
                                                    stackObjects[index].valueType = stackObjects[index].longValue <= stackObjects[stackIndex].longValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                                case ScriptValue.doubleValueType:
                                                    stackObjects[index].valueType = stackObjects[index].longValue <= stackObjects[stackIndex].doubleValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                                default:
                                                    stackObjects[index].valueType = stackObjects[index].longValue <= stackObjects[stackIndex].ToLong() ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index].valueType = stackObjects[index].scriptValue.LessOrEqual(stackObjects[stackIndex]) ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【<=】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.Greater: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.doubleValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.doubleValueType) {
                                                stackObjects[index].valueType = stackObjects[index].doubleValue > stackObjects[stackIndex].doubleValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            } else {
                                                stackObjects[index].valueType = stackObjects[index].doubleValue > stackObjects[stackIndex].ToDouble() ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.longValueType: {
                                            switch (stackObjects[stackIndex].valueType) {
                                                case ScriptValue.longValueType:
                                                    stackObjects[index].valueType = stackObjects[index].longValue > stackObjects[stackIndex].longValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                                case ScriptValue.doubleValueType:
                                                    stackObjects[index].valueType = stackObjects[index].longValue > stackObjects[stackIndex].doubleValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                                default:
                                                    stackObjects[index].valueType = stackObjects[index].longValue > stackObjects[stackIndex].ToLong() ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index].valueType = stackObjects[index].scriptValue.Greater(stackObjects[stackIndex]) ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【>】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.GreaterOrEqual: {
                                    index = stackIndex - 1;
                                    switch (stackObjects[index].valueType) {
                                        case ScriptValue.doubleValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.doubleValueType) {
                                                stackObjects[index].valueType = stackObjects[index].doubleValue >= stackObjects[stackIndex].doubleValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            } else {
                                                stackObjects[index].valueType = stackObjects[index].doubleValue >= stackObjects[stackIndex].ToDouble() ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.longValueType: {
                                            switch (stackObjects[stackIndex].valueType) {
                                                case ScriptValue.longValueType:
                                                    stackObjects[index].valueType = stackObjects[index].longValue >= stackObjects[stackIndex].longValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                                case ScriptValue.doubleValueType:
                                                    stackObjects[index].valueType = stackObjects[index].longValue >= stackObjects[stackIndex].doubleValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                                default:
                                                    stackObjects[index].valueType = stackObjects[index].longValue >= stackObjects[stackIndex].ToLong() ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index].valueType = stackObjects[index].scriptValue.GreaterOrEqual(stackObjects[stackIndex]) ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【>=】运算符不支持当前类型 : {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.Equal: {
                                    index = stackIndex - 1;
                                    valueType = stackObjects[index].valueType;
                                    switch (valueType) {
                                        case ScriptValue.doubleValueType: {
                                            switch (stackObjects[stackIndex].valueType) {
                                                case ScriptValue.doubleValueType:
                                                    stackObjects[index].valueType = stackObjects[index].doubleValue == stackObjects[stackIndex].doubleValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                                case ScriptValue.longValueType:
                                                    stackObjects[index].valueType = stackObjects[index].doubleValue == stackObjects[stackIndex].longValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                                default:
                                                    stackObjects[index].valueType = ScriptValue.falseValueType;
                                                    break;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.stringValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.stringValueType) {
                                                stackObjects[index].valueType = stackObjects[index].stringValue == stackObjects[stackIndex].stringValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            } else {
                                                stackObjects[index].valueType = ScriptValue.falseValueType;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.trueValueType:
                                        case ScriptValue.nullValueType:
                                        case ScriptValue.falseValueType: {
                                            stackObjects[index].valueType = valueType == stackObjects[stackIndex].valueType ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.longValueType: {
                                            switch (stackObjects[stackIndex].valueType) {
                                                case ScriptValue.longValueType:
                                                    stackObjects[index].valueType = stackObjects[index].longValue == stackObjects[stackIndex].longValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                                case ScriptValue.doubleValueType:
                                                    stackObjects[index].valueType = stackObjects[index].longValue == stackObjects[stackIndex].doubleValue ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                                    break;
                                                default:
                                                    stackObjects[index].valueType = ScriptValue.falseValueType;
                                                    break;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index].valueType = stackObjects[index].scriptValue.Equals(stackObjects[stackIndex]) ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.objectValueType: {
                                            stackObjects[index].valueType = stackObjects[index].objectValue.Equals(stackObjects[stackIndex].Value) ? ScriptValue.trueValueType : ScriptValue.falseValueType;
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【==】未知的数据类型 {stackObjects[index].ValueTypeName}");
                                    }
                                }
                                case Opcode.NotEqual: {
                                    index = stackIndex - 1;
                                    valueType = stackObjects[index].valueType;
                                    switch (valueType) {
                                        case ScriptValue.doubleValueType: {
                                            switch (stackObjects[stackIndex].valueType) {
                                                case ScriptValue.doubleValueType:
                                                    stackObjects[index].valueType = stackObjects[index].doubleValue == stackObjects[stackIndex].doubleValue ? ScriptValue.falseValueType : ScriptValue.trueValueType;
                                                    break;
                                                case ScriptValue.longValueType:
                                                    stackObjects[index].valueType = stackObjects[index].doubleValue == stackObjects[stackIndex].longValue ? ScriptValue.falseValueType : ScriptValue.trueValueType;
                                                    break;
                                                default:
                                                    stackObjects[index].valueType = ScriptValue.trueValueType;
                                                    break;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.stringValueType: {
                                            if (stackObjects[stackIndex].valueType == ScriptValue.stringValueType) {
                                                stackObjects[index].valueType = stackObjects[index].stringValue == stackObjects[stackIndex].stringValue ? ScriptValue.falseValueType : ScriptValue.trueValueType;
                                            } else {
                                                stackObjects[index].valueType = ScriptValue.trueValueType;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.trueValueType:
                                        case ScriptValue.nullValueType:
                                        case ScriptValue.falseValueType: {
                                            stackObjects[index].valueType = valueType == stackObjects[stackIndex].valueType ? ScriptValue.falseValueType : ScriptValue.trueValueType;
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.longValueType: {
                                            switch (stackObjects[stackIndex].valueType) {
                                                case ScriptValue.longValueType:
                                                    stackObjects[index].valueType = stackObjects[index].longValue == stackObjects[stackIndex].longValue ? ScriptValue.falseValueType : ScriptValue.trueValueType;
                                                    break;
                                                case ScriptValue.doubleValueType:
                                                    stackObjects[index].valueType = stackObjects[index].longValue == stackObjects[stackIndex].doubleValue ? ScriptValue.falseValueType : ScriptValue.trueValueType;
                                                    break;
                                                default:
                                                    stackObjects[index].valueType = ScriptValue.trueValueType;
                                                    break;
                                            }
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.scriptValueType: {
                                            stackObjects[index].valueType = stackObjects[index].scriptValue.Equals(stackObjects[stackIndex]) ? ScriptValue.falseValueType : ScriptValue.trueValueType;
                                            --stackIndex;
                                            continue;
                                        }
                                        case ScriptValue.objectValueType: {
                                            stackObjects[index].valueType = stackObjects[index].objectValue.Equals(stackObjects[stackIndex].Value) ? ScriptValue.falseValueType : ScriptValue.trueValueType;
                                            --stackIndex;
                                            continue;
                                        }
                                        default: throw new ExecutionException($"【!=】未知的数据类型 {stackObjects[index].ValueTypeName}");
                                    }
                                }
                            }
                            continue;
                        case OpcodeType.Jump:
                            switch (opcode) {
                                case Opcode.Jump: iInstruction = opvalue; continue;
                                case Opcode.Pop: --stackIndex; continue;
                                case Opcode.PopNumber: stackIndex -= opvalue; continue;
                                case Opcode.Call: {
                                    var func = stackObjects[stackIndex--];
                                    for (var i = opvalue - 1; i >= 0; --i) {
                                        parameters[i] = stackObjects[stackIndex--];
                                    }
#if SCORPIO_DEBUG
                                    m_script.PushStackInfo(m_Breviary, instruction.line);
                                    try {
                                        stackObjects[++stackIndex] = func.Call(ScriptValue.Null, parameters, opvalue);
                                    } finally {
                                        m_script.PopStackInfo();
                                    }
#else
                                    stackObjects[++stackIndex] = func.Call(ScriptValue.Null, parameters, opvalue);
#endif
                                    continue;
                                }
                                case Opcode.CallVi: {
                                    var func = stackObjects[stackIndex--];
                                    for (var i = opvalue - 1; i >= 0; --i) {
                                        parameters[i] = stackObjects[stackIndex--];
                                    }
#if SCORPIO_DEBUG
                                    m_script.PushStackInfo(m_Breviary, instruction.line);
                                    try {
                                        stackObjects[++stackIndex] = func.Call(parent, parameters, opvalue);
                                    } finally {
                                        m_script.PopStackInfo();
                                    }
#else
                                    stackObjects[++stackIndex] = func.Call(parent, parameters, opvalue);
#endif
                                    continue;
                                }
                                case Opcode.CallEach: {
                                    index = stackIndex - 1;
                                    var value = stackObjects[stackIndex];
                                    stackObjects[++stackIndex] = value.Call(stackObjects[index]);
                                    continue;
                                }
                                case Opcode.TrueTo: {
                                    switch (stackObjects[stackIndex].valueType) {
                                        case ScriptValue.falseValueType:
                                        case ScriptValue.nullValueType: {
                                            --stackIndex;
                                            continue;
                                        }
                                        default: {
                                            iInstruction = opvalue;
                                            --stackIndex;
                                            continue;
                                        }
                                    }
                                }
                                case Opcode.FalseTo: {
                                    switch (stackObjects[stackIndex].valueType) {
                                        case ScriptValue.falseValueType:
                                        case ScriptValue.nullValueType: {
                                            iInstruction = opvalue;
                                            --stackIndex;
                                            continue;
                                        }
                                        default: {
                                            --stackIndex;
                                            continue;
                                        }
                                    }
                                }
                                case Opcode.TrueLoadTrue: if (stackObjects[stackIndex].valueType == ScriptValue.trueValueType) { iInstruction = opvalue; } else { --stackIndex; } continue;
                                case Opcode.FalseLoadFalse:
                                    switch (stackObjects[stackIndex].valueType) {
                                        case ScriptValue.falseValueType:
                                            iInstruction = opvalue;
                                            continue;
                                        case ScriptValue.nullValueType:
                                            stackObjects[stackIndex].valueType = ScriptValue.falseValueType;
                                            iInstruction = opvalue;
                                            continue;
                                        default:
                                            --stackIndex;
                                            continue;
                                    }
                                case Opcode.RetNone: return ScriptValue.Null;
                                case Opcode.Ret: return stackObjects[stackIndex--];
                                case Opcode.CallUnfold: {
                                    var func = stackObjects[stackIndex--];      //函数对象
                                    var value = constLong[opvalue];             //值 前8位为 参数个数  后56位标识 哪个参数需要展开
                                    var unfold = value & 0xff;                  //折叠标志位
                                    var funcParameterCount = (int)(value >> 8); //参数个数
                                    var startIndex = stackIndex - funcParameterCount + 1;
                                    var parameterIndex = 0;
                                    for (var i = 0; i < funcParameterCount; ++i) {
                                        var parameter = stackObjects[startIndex + i];
                                        if ((unfold & (1L << i)) != 0) {
                                            var array = parameter.Get<ScriptArray>();
                                            if (array != null) {
                                                var values = array.getObjects();
                                                var valueLength = array.Length();
                                                for (var j = 0; j < valueLength; ++j) {
                                                    parameters[parameterIndex++] = values[j];
                                                }
                                            } else {
                                                parameters[parameterIndex++] = parameter;
                                            }
                                        } else {
                                            parameters[parameterIndex++] = parameter;
                                        }
                                    }
                                    stackIndex -= funcParameterCount;
#if SCORPIO_DEBUG
                                    m_script.PushStackInfo(m_Breviary, instruction.line);
                                    try {
                                        stackObjects[++stackIndex] = func.Call(ScriptValue.Null, parameters, parameterIndex);
                                    } finally {
                                        m_script.PopStackInfo();
                                    }
#else
                                    stackObjects[++stackIndex] = func.Call(ScriptValue.Null, parameters, parameterIndex);
#endif
                                    continue;
                                }
                                case Opcode.CallViUnfold: {
                                    var func = stackObjects[stackIndex--];      //函数对象
                                    var value = constLong[opvalue];             //值 前8位为 参数个数  后56位标识 哪个参数需要展开
                                    var unfold = value & 0xff;                  //折叠标志位
                                    var funcParameterCount = (int)(value >> 8); //参数个数
                                    var startIndex = stackIndex - funcParameterCount + 1;
                                    var parameterIndex = 0;
                                    for (var i = 0; i < funcParameterCount; ++i) {
                                        var parameter = stackObjects[startIndex + i];
                                        if ((unfold & (1L << i)) != 0) {
                                            var array = parameter.Get<ScriptArray>();
                                            if (array != null) {
                                                var values = array.getObjects();
                                                var valueLength = array.Length();
                                                for (var j = 0; j < valueLength; ++j) {
                                                    parameters[parameterIndex++] = values[j];
                                                }
                                            } else {
                                                parameters[parameterIndex++] = parameter;
                                            }
                                        } else {
                                            parameters[parameterIndex++] = parameter;
                                        }
                                    }
                                    stackIndex -= funcParameterCount;
#if SCORPIO_DEBUG
                                    m_script.PushStackInfo(m_Breviary, instruction.line);
                                    try {
                                        stackObjects[++stackIndex] = func.Call(parent, parameters, parameterIndex);
                                    } finally {
                                        m_script.PopStackInfo();
                                    }
#else
                                    stackObjects[++stackIndex] = func.Call(parent, parameters, parameterIndex);
#endif
                                    continue;
                                }
                                case Opcode.ExistTo: {
                                    switch (stackObjects[stackIndex].valueType) {
                                        case ScriptValue.nullValueType: {
                                            --stackIndex;
                                            continue;
                                        }
                                        default: {
                                            iInstruction = opvalue;
                                            continue;
                                        }
                                    }
                                }
                            }
                            continue;
                        case OpcodeType.New:
                            switch (opcode) {
                                case Opcode.NewMap: {
                                    var map = new ScriptMap(m_script);
                                    for (var i = opvalue - 1; i >= 0; --i) {
                                        map.SetValue(stackObjects[stackIndex - i].stringValue, stackObjects[stackIndex - i - opvalue]);
                                    }
                                    stackIndex -= opvalue * 2;
                                    stackObjects[++stackIndex].valueType = ScriptValue.scriptValueType;
                                    stackObjects[stackIndex].scriptValue = map;
                                    continue;
                                }
                                case Opcode.NewArray: {
                                    var array = new ScriptArray(m_script);
                                    for (var i = opvalue - 1; i >= 0; --i) {
                                        array.Add(stackObjects[stackIndex - i]);
                                    }
                                    stackIndex -= opvalue;
                                    stackObjects[++stackIndex].valueType = ScriptValue.scriptValueType;
                                    stackObjects[stackIndex].scriptValue = array;
                                    continue;
                                }
                                case Opcode.NewMapObject: {
                                    var map = new ScriptMap(m_script);
                                    for (var i = opvalue - 1; i >= 0; --i) {
                                        map.SetValue(stackObjects[stackIndex - i].Value, stackObjects[stackIndex - i - opvalue]);
                                    }
                                    stackIndex -= opvalue * 2;
                                    stackObjects[++stackIndex].valueType = ScriptValue.scriptValueType;
                                    stackObjects[stackIndex].scriptValue = map;
                                    continue;
                                }
                                case Opcode.NewFunction: {
                                    var functionData = constContexts[opvalue];
                                    var internals = functionData.m_FunctionData.internals;
                                    var function = new ScriptScriptFunction(functionData);
                                    for (var i = 0; i < internals.Length; ++i) {
                                        var internalIndex = internals[i];
                                        function.SetInternal(internalIndex & 0xffff, internalObjects[internalIndex >> 16]);
                                    }
                                    stackObjects[++stackIndex].valueType = ScriptValue.scriptValueType;
                                    stackObjects[stackIndex].scriptValue = function;
                                    continue;
                                }
                                case Opcode.NewLambadaFunction: {
                                    var functionData = constContexts[opvalue];
                                    var internals = functionData.m_FunctionData.internals;
                                    var function = new ScriptScriptBindFunction(functionData, thisObject);
                                    for (var i = 0; i < internals.Length; ++i) {
                                        var internalIndex = internals[i];
                                        function.SetInternal(internalIndex & 0xffff, internalObjects[internalIndex >> 16]);
                                    }
                                    stackObjects[++stackIndex].valueType = ScriptValue.scriptValueType;
                                    stackObjects[stackIndex].scriptValue = function;
                                    continue;
                                }
                                case Opcode.NewType: {
                                    var classData = constClasses[opvalue];
                                    var parentType = classData.parent >= 0 ? m_global.GetValue(constString[classData.parent]) : m_script.TypeObjectValue;
                                    var className = constString[classData.name];
                                    var type = new ScriptType(className, parentType.valueType == ScriptValue.nullValueType ? m_script.TypeObjectValue : parentType);
                                    var functions = classData.functions;
                                    for (var j = 0; j < functions.Length; ++j) {
                                        var func = functions[j];
                                        var functionData = constContexts[func & 0xffffffff];
                                        var internals = functionData.m_FunctionData.internals;
                                        var function = new ScriptScriptFunction(functionData);
                                        for (var i = 0; i < internals.Length; ++i) {
                                            var internalIndex = internals[i];
                                            function.SetInternal(internalIndex & 0xffff, internalObjects[internalIndex >> 16]);
                                        }
                                        type.SetValue(constString[func >> 32], new ScriptValue(function));
                                    }
                                    stackObjects[++stackIndex].valueType = ScriptValue.scriptValueType;
                                    stackObjects[stackIndex].scriptValue = type;
                                    continue;
                                }
                            }
                            continue;
                    }
                }
            } catch (ExecutionException e) {
                throw new ExecutionStackException($"{m_Breviary}:{instruction.line}({iInstruction})\n    {e.ToString()}");
            } catch (ExecutionStackException e) {
                throw new ExecutionStackException($"{m_Breviary}:{instruction.line}({iInstruction})\n    {e.ToString()}");
            } catch (System.Exception e) {
                throw new ExecutionException($"{m_Breviary}:{instruction.line}({iInstruction}) : {e.ToString()}");
            } finally {
                --VariableValueIndex;
                Logger.debug(stackIndex != -1, "堆栈数据未清空，有泄露情况 : " + stackIndex);
            }
            return ScriptValue.Null;
        }
    }
}
