import org.antlr.v4.runtime.CharStreams
import org.antlr.v4.runtime.CommonTokenStream
import prz.lang.SimpleLexer
import prz.lang.SimpleParser

private fun eval(tree: SimpleParser.ExpressionContext) : Double{
    val value = tree.atom()?.text?.toDouble()
    when{
        tree.OP_1() != null -> {
            return when(tree.OP_1()?.text){
                "*" -> eval(tree.expression(0)) * eval(tree.expression(1))
                "/" -> eval(tree.expression(0)) / eval(tree.expression(1))
                else -> value ?: eval(tree.expression(0))
            }
        }
        tree.OP_2() != null -> {
            return when(tree.OP_2()?.text){
                "+" -> eval(tree.expression(0)) + eval(tree.expression(1))
                "-" -> eval(tree.expression(0)) - eval(tree.expression(1))
                else -> value ?: eval(tree.expression(0))
            }
        }
        else ->{
            return value ?: eval(tree.expression(0))
        }
    }
}

fun eval(text: String): Number = text.let {
    val chars = CharStreams.fromString(text)
    val lexer = SimpleLexer(chars)
    val tokenStream = CommonTokenStream(lexer)
    val parser = SimpleParser(tokenStream)
    return eval(parser.expression())
}

fun main(){
    println(eval("5"))
    println(eval("999"))
    println(eval("1 + 6 * 10"))
    println(eval("(1 + 6) * 10"))
}