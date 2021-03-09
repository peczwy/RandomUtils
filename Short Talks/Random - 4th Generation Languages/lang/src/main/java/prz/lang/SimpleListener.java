// Generated from C:/Users/MPenar-Kloc/Documents/Projekty/RandomUtils/Short Talks/Random - 4th Generation Languages/lang/src/main/resources\Simple.g4 by ANTLR 4.7.2

package prz.lang;

import org.antlr.v4.runtime.tree.ParseTreeListener;

/**
 * This interface defines a complete listener for a parse tree produced by
 * {@link SimpleParser}.
 */
public interface SimpleListener extends ParseTreeListener {
	/**
	 * Enter a parse tree produced by {@link SimpleParser#atom}.
	 * @param ctx the parse tree
	 */
	void enterAtom(SimpleParser.AtomContext ctx);
	/**
	 * Exit a parse tree produced by {@link SimpleParser#atom}.
	 * @param ctx the parse tree
	 */
	void exitAtom(SimpleParser.AtomContext ctx);
	/**
	 * Enter a parse tree produced by {@link SimpleParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExpression(SimpleParser.ExpressionContext ctx);
	/**
	 * Exit a parse tree produced by {@link SimpleParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExpression(SimpleParser.ExpressionContext ctx);
}