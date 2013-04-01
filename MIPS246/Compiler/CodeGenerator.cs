﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MIPS246.Core.DataStructure;

namespace MIPS246.Core.Compiler
{
    public class CodeGenerator
    {
        #region Fields
        private static string[] registers = { "T0", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T7", "T9" };
        private RegContent regUseTable;
        #endregion

        #region Constructor
        public CodeGenerator()
        {
            regUseTable = new RegContent(registers.ToList());
        }
        #endregion

        #region Public Method
        public void Generate(List<FourExp> fourExpList, VarTable varTable, List<Instruction> insList, List<DataInstruction> dataInsList, Dictionary<int, String> labelDic)
        {
            //为变量分配内存,并对符号的后续引用信息域和活跃信息域进行初始化
            List<string> varNameList = varTable.GetNames();
            initVarTable(ref varTable, fourExpList, varNameList);
            
            //生成数据段
            genDataIns(varNameList, varTable, dataInsList);

            //遍历四元式表，生成代码段
            int labelNo = 0;
            int count = 0;
            foreach (FourExp f in fourExpList)
            {
                foreach (string varName in varNameList)
                {
                    //从符号表的后续引用信息域和活跃域中去除无用信息
                    if (varTable.GetPeekRefeInfo(varName) == count)
                    {
                        varTable.PopRefeInfo(varName);
                        varTable.PopActInfo(varName);
                    }
                }
                genLabel(f, ref labelNo, ref labelDic);
                convert(f);
                optimize();
            }
        }

        //初始化变量表
        private void initVarTable(ref VarTable varTable, List<FourExp> fourExpList, List<string> varNameList)
        {
            
            int address = 0x0000;
            foreach (string varName in varNameList)
            {
                //初始化变量表中后续引用信息域和活跃信息域
                varTable.SetAddr(varName, address);
                address += 4;
                varTable.ClearRefeInfo(varName);
                varTable.ClearActInfo(varName);
                varTable.PushActInfo(varName, false);
                varTable.PushRefeInfo(varName, -1);
            }
            //扫描四元式表，在变量表中填入相关信息
            int count = fourExpList.Count;
            int length = count;
            for (int i = length; i != 0; i--)
            {
                string A = fourExpList[i].Result;
                string B = fourExpList[i].Arg1;
                string C = fourExpList[i].Arg2;
                if (A != "")
                {
                    varTable.PushRefeInfo(A, -1);
                    varTable.PushActInfo(A, false);
                }
                if (B != "")
                {
                    varTable.PushRefeInfo(B, count);
                    varTable.PushActInfo(B, true);
                }
                if (C != "")
                {
                    varTable.PushRefeInfo(C, count);
                    varTable.PushActInfo(C, true);
                }
                count--;
            }
        }

        ////获取寄存器
        //private string getReg(FourExp f, VarTable varTable, RegContent regUseTable) 
        //{
        //    string A = f.Result;
        //    string B = f.Arg1;
        //    string C = f.Arg2;
        //    List<string> BRegList = varTable.GetAddrInfo(B);
        //    if (BRegList != null)
        //    {
        //        foreach (string BReg in BRegList)
        //        {
        //            List<string> regContent = regUseTable.GetContent(BReg);
        //            if (regContent.Count == 1 || B == A || varTable.GetProp(B).VarActInfo.Peek() == false)
        //            {
        //                return BReg;
        //            }
        //        }
        //    }
        //    if (getNullReg() != null)
        //    {
        //        string reg = getNullReg();
        //        return reg;
        //    }
        //    else
        //    {
        //        Random r = new Random();
        //        int i = r.Next(registers.Length);
        //        string returnReg = registers[i];
        //        doAdjust(A, B, C, returnReg, varTable, regUseTable);
        //        return returnReg;
        //    }
        //}

        ////返回一个未用寄存器
        //private string getNullReg()
        //{
        //    foreach (string reg in registers.ToList())
        //    {
        //        if (regUseTable.GetContent(reg).Count == 0)
        //            return reg;
        //    }
        //    return null;
        //}

        ////返回已用寄存器之后做调整工作
        //private void doAdjust(string A, string B, string C, string returnReg, VarTable varTable, RegContent regUseTable)
        //{
        //    List<string> varList = regUseTable.GetContent(returnReg);
        //    foreach (string M in varList)
        //    {
        //        if (M != A || (M == A && M == C && M != B && !varList.Contains(B)))
        //        {
        //            if (M != A)
        //            {
        //                if ((M == B || M == C) && varList.Contains(B))
        //                {
        //                    varTable.GetProp(M).VarAddrInfo.Clear();
        //                    varTable.GetProp(M).VarAddrInfo.Add(returnReg);
        //                }
        //                else
        //                {
        //                    varTable.GetProp(M).VarAddrInfo.Clear();
        //                }
        //            }
        //            varList.Remove(M);
        //        }
        //    }
        //}

        //生成标签
        private void genLabel(FourExp f, ref int labelNo, ref Dictionary<int, String> labelDic)
        {
            int fourExpNo = f.NextFourExp;
            if (fourExpNo != -1)
            {
                labelDic.Add(fourExpNo, "L" + labelNo.ToString("D3"));
                labelNo++;
            }
        }
     
        //生成指令段
        private void convert(FourExp f)
        {
            switch (f.Op)
            {
                case FourExpOperation.jmp:  //无条件跳转
                    
                    break;
                case FourExpOperation.je:   //条件跳转：=

                    break;
                case FourExpOperation.jne:  //条件跳转：！=

                    break;
                case FourExpOperation.jg:   //条件跳转：>

                    break;
                case FourExpOperation.jge:  //条件跳转：>=

                    break;
                case FourExpOperation.jl:   //条件跳转：<

                    break;
                case FourExpOperation.jle:  //条件跳转：<=


                    break;
                case FourExpOperation.mov:  //赋值

                    break;
                case FourExpOperation.add:  //加

                    break;
                case FourExpOperation.sub:  //减

                    break;
                case FourExpOperation.mul:  //乘

                    break;
                case FourExpOperation.div:  //除

                    break;
                case FourExpOperation.neg:  //取反

                    break;
                case FourExpOperation.and:  //与

                    break;
                case FourExpOperation.or:   //或

                    break;
                case FourExpOperation.not:  //非

                    break;
                default:
                    //错误处理
                    break;
            }
        }

        //生成数据段
        private void genDataIns(List<string> varNameList, VarTable varTable, List<DataInstruction> DataInsList)
        {
            foreach (string varName in varNameList)
            {
                int varValue = varTable.GetValue(varName);
                string varType = "word";
                DataInsList.Add(new DataInstruction(varName, varType, varValue));
            }
        }

        //优化
        private void optimize()
        { 
        
        }
        #endregion
    }
}
