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
        public void Generate(List<FourExp> fourExpList, VarTable varTable, List<string> cmdList, Dictionary<int, String> labelDic, bool isStand)
        {
            //为变量分配内存,并对符号的后续引用信息域和活跃信息域进行初始化
            List<string> varNameList = varTable.GetNames();
            initVarTable(varTable, fourExpList, varNameList);
            
            //生成数据段
            genDataIns(varNameList, varTable, cmdList, isStand);

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
                genLabel(f, ref labelNo, labelDic);
                convert(f, varTable, cmdList);
                optimize();
            }
        }

        //初始化变量表
        private void initVarTable(VarTable varTable, List<FourExp> fourExpList, List<string> varNameList)
        {
            
            short address = 0x0000;
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

        //获取寄存器，isResult为true，则说明需要返回的是存放结果的寄存器
        private string getReg(FourExp f, VarTable varTable, bool isResult, List<string> cmdList)
        {
            //返回B或者C所在的寄存器
            if (isResult)
            {
                if ((varTable.GetAddrInfo(f.Arg1) != "") && (varTable.GetPeekActInfo(f.Arg1) == false))
                {
                    return varTable.GetAddrInfo(f.Arg1);
                }
                if ((varTable.GetAddrInfo(f.Arg2) != "") && (varTable.GetPeekActInfo(f.Arg2) == false))
                {
                    return varTable.GetAddrInfo(f.Arg2);
                }
            }
            //返回未占用寄存器
            foreach (string regName in registers)
            {
                if (regUseTable.GetContent(regName) == null)
                {
                    return regName;
                }
            }
            //随机返回一个已占用的寄存器
            Random r = new Random();
            while (true)
            {
                int i = r.Next(registers.Length);
                string reg = registers[i];
                List<string> varList = new List<string>() { f.Arg1, f.Arg2, f.Result };
                if (!regUseTable.Contains(reg, varList))
                {
                    //调整变量表和寄存器表中的相关域
                    doAdjust(reg, varTable, cmdList);
                    return reg;
                }
            }
        }

        //调整变量表和寄存器表中的相关域
        private void doAdjust(string regName, VarTable varTable, List<string> cmdList)
        {
            foreach (string varName in regUseTable.GetContent(regName))
            {
                cmdList.Add("SW " + regName + ", " + varTable.GetAddr(varName) + "($ZERO)");
                varTable.SetAddrInfo(varName, "");
            }
            regUseTable.Clear(regName);
        }

        //生成标签
        private void genLabel(FourExp f, ref int labelNo, Dictionary<int, String> labelDic)
        {
            int fourExpNo = f.NextFourExp;
            if (fourExpNo != -1)
            {
                labelDic.Add(fourExpNo, "L" + labelNo.ToString("D3"));
                labelNo++;
            }
        }
     
        //生成指令段
        private void convert(FourExp f, VarTable varTable, List<string> cmdList)
        {
            if (f.Op <= FourExpOperation.jle)
            { 
                
            }
            else if (f.Op == FourExpOperation.mov)
            { 
            
            }
            else if (f.Op <= FourExpOperation.or)//数学或逻辑运算
            { 
                  
            }
            else if (f.Op == FourExpOperation.not)
            {

            }
            else 
            { 
                //错误处理
            }
        }

        //生成数据段
        private void genDataIns(List<string> varNameList, VarTable varTable, List<string> cmdList, bool isStand)
        {
            //生成标准的汇编
            if (isStand)
            {
                cmdList.Add(".data");
                foreach (string varName in varNameList)
                {
                    cmdList.Add(varName + ": .word " + varTable.GetValue(varName));
                }
                cmdList.Add(".text");
            }
            //生成非标准的汇编，只有程序段
            else
            {
                foreach (string varName in varNameList)
                {
                    if (varTable.GetType(varName) == VariableType.INT)
                    {
                        short varValue = (short)varTable.GetValue(varName);
                        short varAddr = varTable.GetAddr(varName);
                        cmdList.Add("LUI $T1, " + varValue);
                        cmdList.Add("SW $T1, " + varAddr + "($ZERO)");
                    }
                    else
                    {
                        int value = varTable.GetValue(varName);
                        short high = (short)(value>>16);
                        short varAddr = varTable.GetAddr(varName);
                        cmdList.Add("LUI $TI, " + high);
                        short low = (short)(value & 0xffff);
                        cmdList.Add("ORI $TI, $T1, " + low);
                        cmdList.Add("SW $T1, " + varAddr + "($ZERO)");
                    }
                }
            }
        }

        //优化
        private void optimize()
        { 
        
        }
        #endregion
    }
}
