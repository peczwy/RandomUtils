import org.apache.calcite.sql.SqlIdentifier
import org.apache.calcite.sql.SqlSelect
import org.apache.calcite.sql.parser.SqlParser

fun main(){
    println(1)
    val query = "SELECT * FROM \"file://c:/text.csv\""
    val parser = SqlParser.create(query)
    val sqlNode = parser.parseQuery()
    val sqlSelect = sqlNode as SqlSelect
    val from =  sqlSelect.getFrom() as SqlIdentifier
    println(from.simple)
}